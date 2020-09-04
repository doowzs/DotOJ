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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public interface IModeSubmissionRunner
    {
        public Task<Run> CreateRunAsync(Submission submission, int index, TestCase testCase, bool inline);
        public Task PollRunAsync(Run run);
        public Task DeleteRunsAsync(List<Run> runs);
        public Task<Result> BeforeRunning(Submission submission, Problem problem);
        public Task<Result> OnRunFailed(Submission submission, Problem problem, Run run);
        public Task<Result> RunSubmissionAsync(Submission submission, Problem problem);
    }

    public abstract class ModeSubmissionRunnerBase<T> : IModeSubmissionRunner where T : class
    {
        private const int JudgePollCount = 100;

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

        public async Task<Run> CreateRunAsync(Submission submission, int index, TestCase testCase, bool inline)
        {
            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

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
                TimeLimit = (int) options.CpuTimeLimit,
                Token = token.Token,
                Verdict = Verdict.Running
            };
        }

        public async Task PollRunAsync(Run run)
        {
            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

            for (int i = 0; i < JudgePollCount * 3 && run.Verdict == Verdict.Running; ++i)
            {
                await Task.Delay(run.TimeLimit * 1000 / 3);

                var uri = Instance.Endpoint + "/submissions/" + run.Token +
                          "?base64_encoded=true&fields=token,time,wall_time,memory,compile_output,message,status_id";
                using var message = await client.GetAsync(uri);
                if (!message.IsSuccessStatusCode)
                {
                    Logger.LogError($"PollRun FAIL Token={run.Token} Status={message.StatusCode}");
                    throw new Exception($"Polling API call failed with code {message.StatusCode}.");
                }

                var response = JsonConvert.DeserializeObject<RunnerResponse>(await message.Content.ReadAsStringAsync());
                if (response.Verdict > Verdict.Running)
                {
                    string time = response.Verdict == Verdict.TimeLimitExceeded ? response.WallTime : response.Time;

                    run.Verdict = response.Verdict;
                    run.Time = string.IsNullOrEmpty(time) ? (float?) null : float.Parse(time);
                    run.Memory = response.Memory;
                    run.Message = response.Verdict == Verdict.InternalError
                        ? response.Message
                        : response.CompileOutput;
                    Logger.LogInformation($"PollRun succeed Token={run.Token} Verdict={run.Verdict}" +
                                          $" Time={run.Time} Memory={run.Memory}");
                    return;
                }
            }

            throw new TimeoutException("PollRun timeout.");
        }

        public async Task DeleteRunsAsync(List<Run> runs)
        {
            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);
            foreach (var run in runs)
            {
                var uri = Instance.Endpoint + "/submissions/" + run.Token + "?fields=token";
                var response = await client.DeleteAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError($"DeleteRun failed Token={run.Token} Status={response.StatusCode}");
                }
            }
        }

        public virtual Task<Result> BeforeRunning(Submission submission, Problem problem)
        {
            return Task.FromResult<Result>(null);
        }

        public virtual Task<Result> OnRunFailed(Submission submission, Problem problem, Run run)
        {
            return Task.FromResult<Result>(null);
        }

        public async Task<Result> RunSubmissionAsync(Submission submission, Problem problem)
        {
            submission.Verdict = Verdict.Running;
            Context.Submissions.Update(submission);
            await Context.SaveChangesAsync();

            // Override this method to prevent creating runs.
            Result result = await BeforeRunning(submission, problem);
            if (result != null)
            {
                return result;
            }

            if (problem.TestCases.Count <= 0)
            {
                return Result.NoTestCaseFailure;
            }

            var runs = new List<Run>();
            var testCasesPairList = new List<KeyValuePair<List<TestCase>, bool>>
            {
                new KeyValuePair<List<TestCase>, bool>(problem.SampleCases, true),
                new KeyValuePair<List<TestCase>, bool>(problem.TestCases, false)
            };
            int count = 0, total = problem.SampleCases.Count + problem.TestCases.Count;
            foreach (var pair in testCasesPairList)
            {
                int index = 0;
                var testCases = pair.Key;
                var inline = pair.Value;
                foreach (var testCase in testCases)
                {
                    var run = await CreateRunAsync(submission, inline ? 0 : ++index, testCase, inline);
                    Logger.LogInformation($"CreateRun succeed Submission={submission.Id}" +
                                          (inline ? $" SampleCase={index}" : $" TestCase={index}") +
                                          " Token={run.Token}");
                    runs.Add(run);

                    await PollRunAsync(run);
                    if (run.Verdict > Verdict.Accepted)
                    {
                        submission.Verdict = run.Verdict;
                        submission.FailedOn = run.Index;
                        Context.Submissions.Update(submission);

                        result = await OnRunFailed(submission, problem, run);
                        if (result != null)
                        {
                            break;
                        }
                    }

                    submission.Progress = ++count * 100 / total;
                    await Context.SaveChangesAsync();
                }

                if (result != null)
                {
                    break;
                }
            }

            count = runs.Count(r => r.Index > 0 && r.Verdict == Verdict.Accepted);
            total = problem.TestCases.Count;
            float time = 0, memory = 0;
            var failed = runs.FirstOrDefault(r => r.Verdict > Verdict.Accepted);

            foreach (var run in runs)
            {
                if (run.Time.HasValue)
                {
                    time = Math.Max(time, run.Time.Value);
                }

                if (run.Memory.HasValue)
                {
                    memory = Math.Max(memory, run.Memory.Value);
                }
            }

            await DeleteRunsAsync(runs);
            return new Result
            {
                // If there was any failure, submission's verdict will be changed from Running.
                Verdict = submission.Verdict == Verdict.Running ? Verdict.Accepted : submission.Verdict,
                Time = (int) Math.Min(time * 1000, runs[0].TimeLimit),
                Memory = (int) Math.Min(memory, problem.MemoryLimit),
                FailedOn = failed?.Index,
                Score = count * 100 / total,
                Message = ""
            };
        }
    }
}