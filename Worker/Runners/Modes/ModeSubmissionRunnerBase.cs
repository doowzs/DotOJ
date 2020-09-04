using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public interface IModeSubmissionRunner
    {
        public Task<Run> CreateRunAsync
            (HttpClient client, Submission submission, int index, TestCase testCase, bool inline);

        public Task<List<Run>> CreateRunsAsync(Submission submission, Problem problem);
        public Task PollRunsAsync(List<Run> runs);
        public Task<Result> OnBeforeCreatingRuns(Submission submission, Problem problem);
        public Task<Result> OnPollingRunsComplete(Submission submission, Problem problem, List<Run> runs);
        public Task<Result> RunSubmissionAsync(Submission submission, Problem problem);
    }

    public abstract class ModeSubmissionRunnerBase<T> : IModeSubmissionRunner where T : class
    {
        private const int JudgeTimeLimit = 300;

        protected readonly ApplicationDbContext Context;
        protected readonly IHttpClientFactory Factory;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly JudgeInstance Instance;
        protected readonly ILogger<T> Logger;

        public ModeSubmissionRunnerBase(IServiceProvider provider)
        {
            Context = provider.GetRequiredService<ApplicationDbContext>();
            Factory = provider.GetRequiredService<IHttpClientFactory>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Logger = provider.GetRequiredService<ILogger<T>>();
            Instance = Options.Value.Instance;
        }

        # region Create and Poll Runs

        public async Task<Run> CreateRunAsync
            (HttpClient client, Submission submission, int index, TestCase testCase, bool inline)
        {
            RunnerOptions options;
            if (inline)
            {
                options = new RunnerOptions(submission, testCase.Input, testCase.Output);
            }
            else
            {
                var inputFile =
                    Path.Combine(Options.Value.DataPath, submission.ProblemId.ToString(), testCase.Input);
                var outputFile =
                    Path.Combine(Options.Value.DataPath, submission.ProblemId.ToString(), testCase.Output);

                await using (var inputFileStream = new FileStream(inputFile, FileMode.Open))
                await using (var outputFileStream = new FileStream(outputFile, FileMode.Open))
                await using (var inputMemoryStream = new MemoryStream())
                await using (var outputMemoryStream = new MemoryStream())
                {
                    await inputFileStream.CopyToAsync(inputMemoryStream);
                    await outputFileStream.CopyToAsync(outputMemoryStream);
                    var input = Convert.ToBase64String(inputMemoryStream.GetBuffer());
                    var output = Convert.ToBase64String(outputMemoryStream.GetBuffer());
                    options = new RunnerOptions(submission, input, output);
                }
            }

            using var stringContent = new StringContent(JsonConvert.SerializeObject(options),
                Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response =
                await client.PostAsync(Instance.Endpoint + "/submissions?base64_encoded=true", stringContent);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"CreateRun failed Submission={submission.Id}" +
                                $" Index={index} Status={response.StatusCode}");
                throw new Exception($"CreateRun API call failed with code {response.StatusCode}.");
            }

            var token = JsonConvert.DeserializeObject<RunnerToken>(await response.Content.ReadAsStringAsync());
            return new Run
            {
                Index = index,
                Token = token.Token,
                Verdict = Verdict.Running
            };
        }

        public async Task<List<Run>> CreateRunsAsync(Submission submission, Problem problem)
        {
            var runInfos = new List<Run>();

            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

            foreach (var testCase in problem.SampleCases)
            {
                runInfos.Add(await CreateRunAsync(client, submission, 0, testCase, true));
            }

            int index = 0;
            foreach (var testCase in problem.TestCases)
            {
                runInfos.Add(await CreateRunAsync(client, submission, ++index, testCase, false));
            }

            return runInfos;
        }

        public async Task PollRunsAsync(List<Run> runs)
        {
            var tokens = new List<string>();
            foreach (var run in runs)
            {
                if (run.Verdict == Verdict.Running)
                {
                    tokens.Add(run.Token);
                }
            }

            if (!tokens.IsNullOrEmpty())
            {
                using var client = Factory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
                client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

                var uri = Instance.Endpoint + "/submissions/batch" +
                          "?base64_encoded=true&tokens=" + string.Join(",", tokens) +
                          "&fields=token,time,wall_time,memory,compile_output,message,status_id";
                using var response = await client.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError($"PollRuns FAIL Tokens={string.Join(",", tokens)} Status={response.StatusCode}");
                    throw new Exception($"Polling API call failed with code {response.StatusCode}.");
                }

                var runnerResponse =
                    JsonConvert.DeserializeObject<RunnerResponse>(await response.Content.ReadAsStringAsync());
                foreach (var status in runnerResponse.Statuses)
                {
                    var run = runs.Find(r => r.Token == status.Token);
                    if (run == null)
                    {
                        throw new Exception("Run not found in runInfos.");
                    }

                    if (status.Verdict > Verdict.Running)
                    {
                        string time = status.Verdict == Verdict.TimeLimitExceeded ? status.WallTime : status.Time;

                        run.Verdict = status.Verdict;
                        run.Time = string.IsNullOrEmpty(time) ? (float?) null : float.Parse(time);
                        run.Memory = status.Memory;
                        run.Message = status.Verdict == Verdict.InternalError
                            ? status.Message
                            : status.CompileOutput;
                    }
                }
            }
        }

        #endregion

        public virtual Task<Result> OnBeforeCreatingRuns(Submission submission, Problem problem)
        {
            return Task.FromResult<Result>(null);
        }

        public virtual Task<Result> OnPollingRunsComplete(Submission submission, Problem problem, List<Run> runs)
        {
            return Task.FromResult<Result>(null);
        }

        public async Task<Result> RunSubmissionAsync(Submission submission, Problem problem)
        {
            submission.Verdict = Verdict.Running;
            Context.Submissions.Update(submission);
            await Context.SaveChangesAsync();
            
            {
                // Override this method to prevent creating runs.
                var result = await OnBeforeCreatingRuns(submission, problem);
                if (result != null)
                {
                    return result;
                }
            }

            var runs = await CreateRunsAsync(submission, problem);
            if (runs.IsNullOrEmpty())
            {
                return Result.NoTestCaseFailure;
            }

            for (int i = 0; i < JudgeTimeLimit; ++i)
            {
                await Task.Delay(1000);
                await PollRunsAsync(runs);

                // Sudden death of Compilation Error and Internal Error.
                var fatal = runs.FirstOrDefault(r =>
                    r.Verdict == Verdict.CompilationError || r.Verdict == Verdict.InternalError);
                if (fatal != null)
                {
                    return new Result
                    {
                        Verdict = fatal.Verdict,
                        Time = null, Memory = null,
                        FailedOn = 0, Score = 0,
                        Message = fatal.Message
                    };
                }

                // Override this method to provide custom data handling.
                var result = await OnPollingRunsComplete(submission, problem, runs);
                if (result != null)
                {
                    return result;
                }

                var total = runs.Count;
                var finished = runs.Count(r => r.Verdict > Verdict.Running);
                submission.Progress = finished * 100 / total;
                await Context.SaveChangesAsync();
            }

            return Result.TimeoutFailure;
        }
    }
}