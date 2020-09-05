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
    public sealed class PlainRunnerBase : TestCaseRunnerBase, IDisposable
    {
        private const int PollLimit = 100;

        private readonly Contest _contest;
        private readonly Problem _problem;
        private readonly Submission _submission;

        private readonly ApplicationDbContext _context;
        private readonly IOptions<JudgingConfig> _options;
        private readonly ILogger<PlainRunnerBase> _logger;
        private readonly HttpClient _client;

        public PlainRunnerBase(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            _contest = contest;
            _problem = problem;
            _submission = submission;

            _context = provider.GetRequiredService<ApplicationDbContext>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _logger = provider.GetRequiredService<ILogger<PlainRunnerBase>>();

            _client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
            _client.DefaultRequestHeaders.Add("X-Auth-User", _options.Value.Instance.AuthUser);
            _client.DefaultRequestHeaders.Add("X-Auth-Token", _options.Value.Instance.AuthToken);
        }

        public override async Task<Result> RunSubmissionAsync()
        {
            Result result = null;
            if (_problem.TestCases.Count <= 0)
            {
                return Result.NoTestCaseFailure;
            }

            _submission.Verdict = Verdict.Running;
            _context.Submissions.Update(_submission);
            await _context.SaveChangesAsync();

            if (BeforeStartDelegate != null)
            {
                result = await BeforeStartDelegate.Invoke(_contest, _submission, _problem);
                if (result != null)
                {
                    return result;
                }
            }

            var runs = new List<Run>();
            var testCasesPairList = new List<KeyValuePair<List<TestCase>, bool>>
            {
                new KeyValuePair<List<TestCase>, bool>(_problem.SampleCases, true),
                new KeyValuePair<List<TestCase>, bool>(_problem.TestCases, false)
            };
            int count = 0, total = _problem.SampleCases.Count + _problem.TestCases.Count;
            foreach (var pair in testCasesPairList)
            {
                int index = 0;
                var testCases = pair.Key;
                var inline = pair.Value;

                if (BeforeTestGroupDelegate != null)
                {
                    result = await BeforeTestGroupDelegate.Invoke(_contest, _submission, _problem, inline);
                    if (result != null)
                    {
                        return result;
                    }
                }

                foreach (var testCase in testCases)
                {
                    var run = await CreateRunAsync(inline ? 0 : ++index, testCase, inline);
                    runs.Add(run);
                    await PollRunAsync(run);
                    await DeleteRunAsync(run);
                    if (run.Verdict > Verdict.Accepted)
                    {
                        _submission.Verdict = run.Verdict;
                        _submission.FailedOn = run.Index;
                        _context.Submissions.Update(_submission);

                        if (OnRunFailedDelegate != null)
                        {
                            result = await OnRunFailedDelegate.Invoke(_contest, _submission, _problem, run);
                            // Do not return immediately, we will save submission data on next lines.
                        }
                    }

                    _submission.Progress = ++count * 100 / total;
                    await _context.SaveChangesAsync();
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            count = runs.Count(r => r.Index > 0 && r.Verdict == Verdict.Accepted);
            total = _problem.TestCases.Count;
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

            return new Result
            {
                // If there was any failure, submission's verdict will be changed from Running.
                Verdict = _submission.Verdict == Verdict.Running ? Verdict.Accepted : _submission.Verdict,
                Time = (int) Math.Min(time * 1000, runs[0].TimeLimit),
                Memory = (int) Math.Min(memory, _problem.MemoryLimit),
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
                options = new RunnerOptions(_submission, testCase.Input, testCase.Output);
            }
            else
            {
                var inputFile =
                    Path.Combine(_options.Value.DataPath, _problem.Id.ToString(), testCase.Input);
                var outputFile =
                    Path.Combine(_options.Value.DataPath, _problem.Id.ToString(), testCase.Output);

                await using (var inputFileStream = new FileStream(inputFile, FileMode.Open))
                await using (var outputFileStream = new FileStream(outputFile, FileMode.Open))
                await using (var inputMemoryStream = new MemoryStream())
                await using (var outputMemoryStream = new MemoryStream())
                {
                    await inputFileStream.CopyToAsync(inputMemoryStream);
                    await outputFileStream.CopyToAsync(outputMemoryStream);
                    var input = Convert.ToBase64String(inputMemoryStream.GetBuffer());
                    var output = Convert.ToBase64String(outputMemoryStream.GetBuffer());
                    options = new RunnerOptions(_submission, input, output);
                }
            }

            var uri = _options.Value.Instance.Endpoint + "/submissions?base64_encoded=true";
            using var stringContent = new StringContent(JsonConvert.SerializeObject(options),
                Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await _client.PostAsync(uri, stringContent);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"CreateRun failed Submission={_submission.Id}" +
                                 $" Index={index} Status={response.StatusCode}");
                throw new Exception($"CreateRun API call failed with code {response.StatusCode}.");
            }

            var token = JsonConvert.DeserializeObject<RunnerToken>(await response.Content.ReadAsStringAsync());
            _logger.LogInformation($"CreateRun succeed Submission={_submission.Id}" +
                                   (inline ? $" SampleCase={index}" : $" TestCase={index}") + $" Token={token}");
            return new Run
            {
                Index = index,
                TimeLimit = (int) options.CpuTimeLimit * 1000,
                Token = token.Token,
                Verdict = Verdict.Running
            };
        }

        private async Task PollRunAsync(Run run)
        {
            for (int i = 0; i < PollLimit * 3 && run.Verdict == Verdict.Running; ++i)
            {
                await Task.Delay(run.TimeLimit / 3);

                var uri = _options.Value.Instance.Endpoint + "/submissions/" + run.Token +
                          "?base64_encoded=true&fields=token,time,wall_time,memory,compile_output,message,status_id";
                using var message = await _client.GetAsync(uri);
                if (!message.IsSuccessStatusCode)
                {
                    _logger.LogError($"PollRun FAIL Token={run.Token} Status={message.StatusCode}");
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
                    _logger.LogInformation($"PollRun succeed Token={run.Token} Verdict={run.Verdict}" +
                                           $" Time={run.Time} Memory={run.Memory}");
                    return;
                }
            }

            throw new TimeoutException("PollRun timeout.");
        }

        private async Task DeleteRunAsync(Run run)
        {
            var uri = _options.Value.Instance.Endpoint + "/submissions/" + run.Token + "?fields=token";
            var response = await _client.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"DeleteRun failed Token={run.Token} Status={response.StatusCode}");
            }
        }

        #endregion

        #region Implement IDisposable to dispose HTTP client

        ~PlainRunnerBase()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Dispose();
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