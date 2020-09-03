using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace WebApp.Services.Judge.Submission
{
    public class PracticeModeJudgeService : LoggableService<PracticeModeJudgeService>, ISubmissionJudgeService
    {
        private const int JudgeTimeLimit = 300;

        protected readonly IHttpClientFactory Factory;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly JudgeInstance Instance;

        public PracticeModeJudgeService(IServiceProvider provider) : base(provider, true)
        {
            Factory = provider.GetRequiredService<IHttpClientFactory>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Instance = Options.Value.Instances[0];
        }

        private async Task<RunInfo> CreateRun
            (HttpClient client, Data.Models.Submission submission, int index, TestCase testCase, bool inline)
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
                await LogError($"CreateRun FAIL Submission={submission.Id} Index={index} Status={response.StatusCode}");
                throw new Exception($"Create API call failed with code {response.StatusCode}.");
            }

            var token = JsonConvert.DeserializeObject<RunnerToken>(await response.Content.ReadAsStringAsync());
            return new RunInfo
            {
                Index = index,
                Token = token.Token,
                Verdict = Verdict.Running
            };
        }

        private async Task<List<RunInfo>> CreateRuns(Data.Models.Submission submission, Problem problem)
        {
            var runInfos = new List<RunInfo>();

            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

            foreach (var testCase in problem.SampleCases)
            {
                runInfos.Add(await CreateRun(client, submission, 0, testCase, true));
            }

            int index = 0;
            foreach (var testCase in problem.TestCases)
            {
                runInfos.Add(await CreateRun(client, submission, ++index, testCase, false));
            }

            return runInfos;
        }

        private async Task<JudgeResult> PollRuns(Data.Models.Submission submission, List<RunInfo> runInfos)
        {
            var tokens = new List<string>();
            foreach (var runInfo in runInfos)
            {
                if (runInfo.Verdict == Verdict.Running)
                {
                    tokens.Add(runInfo.Token);
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
                    await LogError($"PollRuns FAIL Tokens={string.Join(",", tokens)} Status={response.StatusCode}");
                    throw new Exception($"Polling API call failed with code {response.StatusCode}.");
                }

                var runnerResponse =
                    JsonConvert.DeserializeObject<RunnerResponse>(await response.Content.ReadAsStringAsync());
                foreach (var status in runnerResponse.Statuses)
                {
                    var runInfo = runInfos.Find(r => r.Token == status.Token);
                    if (runInfo == null)
                    {
                        throw new Exception("RunInfo not found in runInfos.");
                    }

                    if (status.Verdict == Verdict.CompilationError || status.Verdict == Verdict.InternalError)
                    {
                        // Immediate failure on compilation error or internal error.
                        // There is no need to wait for other responses at all.
                        return new JudgeResult
                        {
                            Verdict = status.Verdict,
                            Score = 0, FailedOn = 0,
                            Message = status.Message
                        };
                    }
                    else if (status.Verdict > Verdict.Running)
                    {
                        runInfo.Verdict = status.Verdict;
                        runInfo.Time = string.IsNullOrEmpty(status.Time) ? (float?) null : float.Parse(status.Time);
                        runInfo.WallTime = string.IsNullOrEmpty(status.WallTime)
                            ? (float?) null
                            : float.Parse(status.WallTime);
                        runInfo.Memory = status.Memory;
                        runInfo.Message = status.Verdict == Verdict.InternalError
                            ? status.Message
                            : status.CompileOutput;

                        if (status.Verdict != Verdict.Accepted && submission.Verdict == Verdict.Running)
                        {
                            // Submission failed on some case, set result for immediate response.
                            // Negative score indicates that service is still judging on other cases.
                            submission.Verdict = status.Verdict;
                            if (!submission.FailedOn.HasValue)
                            {
                                submission.FailedOn = runInfo.Index;
                            }
                            else
                            {
                                submission.FailedOn = Math.Min(submission.FailedOn.Value, runInfo.Index);
                            }
                        }
                    }
                }

                if (submission.Verdict <= Verdict.Running)
                {
                    submission.Verdict = Verdict.Running;
                }
                submission.Progress = runInfos.Count(ri => ri.Verdict > Verdict.Running) * 100 / runInfos.Count;
                Context.Update(submission);
                await Context.SaveChangesAsync();

                return null; // Still has other cases to judge, let caller poll again.
            }
            else
            {
                var verdict = Verdict.Accepted;
                int? failedOn = null;
                int count = 0, total = 0;
                float time = 0.0f, memory = 0.0f;

                foreach (var runInfo in runInfos)
                {
                    if (runInfo.Index > 0)
                    {
                        if (runInfo.Verdict == Verdict.Accepted)
                        {
                            ++count;
                        }

                        ++total;
                    }

                    if (runInfo.Verdict != Verdict.Accepted && !failedOn.HasValue)
                    {
                        verdict = runInfo.Verdict;
                        failedOn = runInfo.Index;
                    }

                    if (runInfo.Time.HasValue)
                    {
                        time = Math.Max(time, runInfo.Time.Value);
                    }

                    if (runInfo.WallTime.HasValue)
                    {
                        time = Math.Max(time, runInfo.WallTime.Value);
                    }

                    if (runInfo.Memory.HasValue)
                    {
                        memory = Math.Max(memory, runInfo.Memory.Value);
                    }
                }

                return new JudgeResult
                {
                    // Do not update verdict again if the submission has already failed before.
                    Verdict = submission.Verdict == Verdict.Running ? verdict : submission.Verdict,
                    Time = (int) (time * 1000),
                    Memory = (int) memory,
                    Message = "",
                    FailedOn = failedOn,
                    Score = total == 0 ? (verdict == Verdict.Accepted ? 100 : 0) : count * 100 / total
                };
            }
        }

        public async Task<JudgeResult> Judge(Data.Models.Submission submission, Problem problem)
        {
            submission.Verdict = Verdict.InQueue;
            submission.FailedOn = null;
            Context.Submissions.Update(submission);
            await Context.SaveChangesAsync();

            var runInfos = await CreateRuns(submission, problem);
            if (runInfos.IsNullOrEmpty())
            {
                return new JudgeResult
                {
                    Verdict = Verdict.Failed,
                    FailedOn = null, Score = 0,
                    Message = "No test cases available."
                };
            }

            for (int i = 0; i < JudgeTimeLimit; ++i)
            {
                await Task.Delay(1000);

                var result = await PollRuns(submission, runInfos);
                if (result != null)
                {
                    // Fix time and memory to be no larger than limit.
                    var timeFactor = RunnerLanguageOptions
                        .LanguageOptionsDict[submission.Program.Language.GetValueOrDefault()].timeFactor;
                    result.Time = Math.Min(result.Time, (int) (problem.TimeLimit * timeFactor));
                    result.Memory = Math.Min(result.Memory, problem.MemoryLimit);
                    return result;
                }
            }

            return new JudgeResult
            {
                Verdict = Verdict.Failed,
                FailedOn = null, Score = 0,
                Message = "Judge timeout."
            };
        }
    }
}