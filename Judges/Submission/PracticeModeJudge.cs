﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Judge1.Judges.Submission
{
    public class PracticeModeJudge : ISubmissionJudge
    {
        private const int JudgeTimeLimit = 300;

        private readonly IHttpClientFactory _factory;
        private readonly IOptions<JudgingConfig> _options;
        private readonly ILogger<PracticeModeJudge> _logger;
        private readonly JudgeInstance _instance;

        public PracticeModeJudge(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IHttpClientFactory>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _logger = provider.GetRequiredService<ILogger<PracticeModeJudge>>();
            _instance = _options.Value.Instances[0];
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
                    Path.Combine(_options.Value.DataPath, submission.ProblemId.ToString(), testCase.Input);
                var outputFile =
                    Path.Combine(_options.Value.DataPath, submission.ProblemId.ToString(), testCase.Output);

                await using (var inputStream = new FileStream(inputFile, FileMode.Open))
                await using (var outputStream = new FileStream(outputFile, FileMode.Open))
                using (var inputReader = new StreamReader(inputStream))
                using (var outputReader = new StreamReader(outputStream))
                {
                    var input = await inputReader.ReadToEndAsync();
                    var output = await outputReader.ReadToEndAsync();
                    options = new RunnerOptions(submission, input, output);
                }
            }

            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Auth-User", _instance.AuthUser);
            client.DefaultRequestHeaders.Add("X-Auth-Token", _instance.AuthToken);

            using var stringContent = new StringContent(JsonConvert.SerializeObject(options),
                Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await client.PostAsync(_instance.Endpoint + "/submissions", stringContent);
            if (!response.IsSuccessStatusCode)
            {
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
                            Verdict = Verdict.CompilationError,
                            FailedOn = 0,
                            Message = runInfo.Message,
                            Time = 0, Memory = 0, Score = 0
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

                    time = Math.Max(time, runInfo.Time);
                    memory = Math.Max(time, runInfo.Memory);
                }

                return new JudgeResult
                {
                    Verdict = verdict,
                    Time = (int) (time * 1000),
                    Memory = (int) memory,
                    Message = "",
                    FailedOn = failedOn,
                    Score = total == 0 ? 100 : count * 100 / total
                };
            }
            else
            {
                using var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Auth-User", _instance.AuthUser);
                client.DefaultRequestHeaders.Add("X-Auth-Token", _instance.AuthToken);

                using var response = await client.GetAsync(_instance.Endpoint + "/submissions/batch" +
                                                           "?tokens=" + string.Join(",", tokens) +
                                                           "&fields=token,time,memory,compile_output,message,status_id");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Polling API call failed with code {response.StatusCode}.");
                }

                _logger.LogInformation(await response.Content.ReadAsStringAsync());
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
                        runInfo.Time = float.Parse(status.Time);
                        runInfo.Memory = status.Memory.GetValueOrDefault();
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
                FailedOn = -1,
                Score = 0
            };
        }
    }
}