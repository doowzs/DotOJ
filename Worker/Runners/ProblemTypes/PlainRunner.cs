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

namespace Worker.Runners.ProblemTypes
{
    public class PlainRunner : IDisposable
    {
        private const int PollLimit = 100;

        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;

        protected readonly ApplicationDbContext Context;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly HttpClient Client;
        protected ILogger Logger;

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        protected Func<Contest, Problem, Submission, Run, Task> OnRunCompleteDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public PlainRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;

            Context = provider.GetRequiredService<ApplicationDbContext>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Logger = provider.GetRequiredService<ILogger<PlainRunner>>();

            Client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
            Client.DefaultRequestHeaders.Add("X-Auth-User", Options.Value.Instance.AuthUser);
            Client.DefaultRequestHeaders.Add("X-Auth-Token", Options.Value.Instance.AuthToken);
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            JudgeResult result = null;
            if (Problem.TestCases.Count <= 0)
            {
                return JudgeResult.NoTestCaseFailure;
            }

            Submission.Verdict = Verdict.Running;
            Context.Submissions.Update(Submission);
            await Context.SaveChangesAsync();

            if (BeforeStartDelegate != null)
            {
                result = await BeforeStartDelegate.Invoke(Contest, Problem, Submission);
                if (result != null)
                {
                    return result;
                }
            }

            var runs = new List<Run>();
            var testCasesPairList = new List<KeyValuePair<List<TestCase>, bool>>
            {
                new KeyValuePair<List<TestCase>, bool>(Problem.SampleCases, true),
                new KeyValuePair<List<TestCase>, bool>(Problem.TestCases, false)
            };
            int count = 0, total = Problem.SampleCases.Count + Problem.TestCases.Count;
            foreach (var pair in testCasesPairList)
            {
                int index = 0;
                var testCases = pair.Key;
                var inline = pair.Value;

                if (BeforeTestGroupDelegate != null)
                {
                    result = await BeforeTestGroupDelegate.Invoke(Contest, Problem, Submission, inline);
                    if (result != null)
                    {
                        return result;
                    }
                }

                foreach (var testCase in testCases)
                {
                    var run = await CreateRunAsync(inline ? 0 : ++index, testCase, inline);
                    await PollRunAsync(run);
                    await DeleteRunAsync(run);

                    if (OnRunCompleteDelegate != null)
                    {
                        await OnRunCompleteDelegate.Invoke(Contest, Problem, Submission, run);
                    }

                    runs.Add(run);
                    if (run.Verdict > Verdict.Accepted)
                    {
                        Submission.Verdict = run.Verdict;
                        Submission.FailedOn = run.Index;
                        Context.Submissions.Update(Submission);

                        if (OnRunFailedDelegate != null)
                        {
                            result = await OnRunFailedDelegate.Invoke(Contest, Problem, Submission, run);
                            // Do not return immediately, we will save submission data on next lines.
                        }
                    }

                    Submission.Progress = ++count * 100 / total;
                    await Context.SaveChangesAsync();
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            count = runs.Count(r => r.Index > 0 && r.Verdict == Verdict.Accepted);
            total = Problem.TestCases.Count;
            int time = 0, memory = 0;
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

            return new JudgeResult
            {
                // If there was any failure, submission's verdict will be changed from Running.
                Verdict = Submission.Verdict == Verdict.Running ? Verdict.Accepted : Submission.Verdict,
                Time = time,
                Memory = memory,
                FailedOn = failed?.Index,
                Score = count * 100 / total,
                Message = ""
            };
        }

        #region Backend manipulation methods

        private async Task<Run> CreateRunAsync(int index, TestCase testCase, bool inline)
        {
            RunnerOptions options;
            if (inline)
            {
                options = new RunnerOptions(Submission, testCase.Input, testCase.Output);
            }
            else
            {
                var inputFile =
                    Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Input);
                var outputFile =
                    Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Output);

                await using (var inputFileStream = new FileStream(inputFile, FileMode.Open))
                await using (var outputFileStream = new FileStream(outputFile, FileMode.Open))
                await using (var inputMemoryStream = new MemoryStream())
                await using (var outputMemoryStream = new MemoryStream())
                {
                    await inputFileStream.CopyToAsync(inputMemoryStream);
                    await outputFileStream.CopyToAsync(outputMemoryStream);
                    var input = Convert.ToBase64String(inputMemoryStream.GetBuffer());
                    var output = Convert.ToBase64String(outputMemoryStream.GetBuffer());
                    options = new RunnerOptions(Submission, input, output);
                }
            }

            var uri = Options.Value.Instance.Endpoint + "/submissions?base64_encoded=true";
            using var stringContent = new StringContent(JsonConvert.SerializeObject(options),
                Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await Client.PostAsync(uri, stringContent);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"CreateRun failed Submission={Submission.Id}" +
                                $" Index={index} Status={response.StatusCode}");
                throw new Exception($"CreateRun API call failed with code {response.StatusCode}.");
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            Logger.LogInformation($"CreateRun succeed Submission={Submission.Id}" +
                                  (inline ? $" SampleCase={index}" : $" TestCase={index}") + $" Token={token}");
            return new Run
            {
                Index = index,
                TimeLimit = (int) options.CpuTimeLimit * 1000,
                Token = token.Token,
                Verdict = Verdict.Running
            };
        }

        protected async Task PollRunAsync(Run run, bool getStdout = false)
        {
            for (int i = 0; i < PollLimit * 3 && run.Verdict == Verdict.Running; ++i)
            {
                await Task.Delay(run.TimeLimit / 3);

                var uri = Options.Value.Instance.Endpoint + "/submissions/" + run.Token +
                          "?base64_encoded=true&fields=token,time,wall_time,memory,compile_output,message,status_id" +
                          (getStdout ? ",stdout" : "");
                using var message = await Client.GetAsync(uri);
                if (!message.IsSuccessStatusCode)
                {
                    Logger.LogError($"PollRun FAIL Token={run.Token} Status={message.StatusCode}");
                    throw new Exception($"Polling API call failed with code {message.StatusCode}.");
                }

                var response = JsonConvert.DeserializeObject<PollResponse>(await message.Content.ReadAsStringAsync());
                if (response.Verdict > Verdict.Running)
                {
                    string time = response.Verdict == Verdict.TimeLimitExceeded ? response.WallTime : response.Time;

                    run.Verdict = response.Verdict;
                    run.Time = string.IsNullOrEmpty(time)
                        ? (int?) null
                        : Math.Min((int) (float.Parse(time) * 1000), run.TimeLimit);
                    run.Memory = response.Memory.HasValue
                        ? (int) Math.Ceiling(response.Memory.Value)
                        : Problem.MemoryLimit;
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

        protected async Task DeleteRunAsync(Run run)
        {
            var uri = Options.Value.Instance.Endpoint + "/submissions/" + run.Token + "?fields=token";
            var response = await Client.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"DeleteRun failed Token={run.Token} Status={response.StatusCode}");
            }
        }

        #endregion

        #region Implement IDisposable to dispose HTTP client

        ~PlainRunner()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}