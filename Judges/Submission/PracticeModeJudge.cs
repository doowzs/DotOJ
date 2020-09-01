using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
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
        private readonly string _endpoint;

        public PracticeModeJudge(IServiceProvider provider)
        {
            _factory = provider.GetRequiredService<IHttpClientFactory>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _logger = provider.GetRequiredService<ILogger<PracticeModeJudge>>();
            _endpoint = _options.Value.Instances[0].Endpoint + "/submissions";
        }

        private async Task<RunInfo> CreateRun(Models.Submission submission, int index, TestCase testCase, bool inline)
        {
            using (var client = _factory.CreateClient())
            await using (var buffer = new MemoryStream())
            {
                using (var sw = new StreamWriter(buffer, Encoding.UTF8, 1000, true))
                using (var jtw = new JsonTextWriter(sw))
                {
                    var serializer = new JsonSerializer();
                    if (inline)
                    {
                        var options = new RunnerOptionsInline(submission, testCase.Input, testCase.Output);
                        serializer.Serialize(jtw, options);
                    }
                    else
                    {
                        var options = new RunnerOptionsFile(submission,
                            Path.Combine(_options.Value.DataPath, submission.ProblemId.ToString(), testCase.Input),
                            Path.Combine(_options.Value.DataPath, submission.ProblemId.ToString(), testCase.Output));
                        serializer.Serialize(jtw, options);
                    }

                    await jtw.FlushAsync();
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, _endpoint))
                using (var content = new StreamContent(buffer))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
                    request.Content = content;
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    _logger.LogInformation(await response.Content.ReadAsStringAsync());
                }
            }

            return new RunInfo
            {
                Index = index,
                Token = "",
                Verdict = Verdict.Accepted
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
            await Task.Delay(1);
            return new JudgeResult
            {
                Verdict = Verdict.Accepted,
                FailedOn = -1, Score = 100
            };
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