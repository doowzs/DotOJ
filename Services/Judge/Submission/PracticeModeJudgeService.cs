using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Judge1.Services.Judge.Submission
{
    public class PracticeModeJudgeService : LoggableService<PracticeModeJudgeService>, ISubmissionJudgeService
    {
        private const int JudgeTimeLimit = 300;

        protected readonly IHttpClientFactory Factory;
        protected readonly IOptions<JudgingConfig> Options;
        protected readonly JudgeInstance Instance;

        public PracticeModeJudgeService(IServiceProvider provider) : base(provider)
        {
            Factory = provider.GetRequiredService<IHttpClientFactory>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Instance = Options.Value.Instances[0];
        }

        private async Task<RunInfo> CreateRun(Models.Submission submission, int index, TestCase testCase, bool inline)
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

            using var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

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

        private async Task<List<RunInfo>> CreateRuns(Models.Submission submission, Problem problem)
        {
            var runInfos = new List<RunInfo>();

            foreach (var testCase in problem.SampleCases)
            {
                runInfos.Add(await CreateRun(submission, 0, testCase, true));
            }

            int index = 0;
            foreach (var testCase in problem.TestCases)
            {
                runInfos.Add(await CreateRun(submission, ++index, testCase, false));
            }

            return runInfos;
        }

        private async Task<JudgeResult> PollRuns(List<RunInfo> runInfos)
        {
            var tokens = new List<string>();
            foreach (var runInfo in runInfos)
            {
                if (runInfo.Verdict == Verdict.Running)
                {
                    tokens.Add(runInfo.Token);
                }
            }

            if (tokens.IsNullOrEmpty())
            {
                var verdict = Verdict.Accepted;
                int failedOn = -1, count = 0, total = 0;
                float time = 0.0f, memory = 0.0f;

                foreach (var runInfo in runInfos)
                {
                    if (runInfo.Verdict == Verdict.CompilationError || runInfo.Verdict == Verdict.InternalError)
                    {
                        return new JudgeResult
                        {
                            Verdict = runInfo.Verdict,
                            FailedOn = 0, Score = 0,
                            Message = runInfo.Message
                        };
                    }

                    if (runInfo.Index > 0)
                    {
                        if (runInfo.Verdict == Verdict.Accepted)
                        {
                            ++count;
                        }

                        ++total;
                    }

                    if (runInfo.Verdict != Verdict.Accepted && failedOn == -1)
                    {
                        verdict = runInfo.Verdict;
                        failedOn = runInfo.Index;
                    }

                    if (runInfo.Time.HasValue)
                    {
                        time = Math.Max(time, runInfo.Time.Value);
                    }

                    if (runInfo.Memory.HasValue)
                    {
                        memory = Math.Max(memory, runInfo.Memory.Value);
                    }
                }

                return new JudgeResult
                {
                    Verdict = verdict,
                    Time = (int) (time * 1000),
                    Memory = (int) memory,
                    Message = "",
                    FailedOn = failedOn,
                    Score = total == 0 ? (verdict == Verdict.Accepted ? 100 : 0) : count * 100 / total
                };
            }
            else
            {
                using var client = Factory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Auth-User", Instance.AuthUser);
                client.DefaultRequestHeaders.Add("X-Auth-Token", Instance.AuthToken);

                using var response = await client.GetAsync(Instance.Endpoint + "/submissions/batch" +
                                                           "?base64_encoded=true&tokens=" + string.Join(",", tokens) +
                                                           "&fields=token,time,memory,compile_output,message,status_id");
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

                    if (status.Verdict > Verdict.Running)
                    {
                        runInfo.Verdict = status.Verdict;
                        runInfo.Time = string.IsNullOrEmpty(status.Time) ? (float?) null : float.Parse(status.Time);
                        runInfo.Memory = status.Memory;
                        runInfo.Message = status.Verdict == Verdict.InternalError
                            ? status.Message
                            : status.CompileOutput;
                    }
                }

                return null;
            }
        }

        public async Task<JudgeResult> Judge(Models.Submission submission, Problem problem)
        {
            var runInfos = await CreateRuns(submission, problem);
            for (int i = 0; i < JudgeTimeLimit; ++i)
            {
                await Task.Delay(1000);
                var result = await PollRuns(runInfos);
                if (result != null)
                {
                    return result;
                }
            }

            return new JudgeResult
            {
                Verdict = Verdict.Failed,
                FailedOn = 0, Score = 0,
                Message = "Judge timeout."
            };
        }
    }
}