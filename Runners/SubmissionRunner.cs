using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Judge1.Data;
using Judge1.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Judge1.Runners
{
    static class Languages
    {
        public class LanguageOptions
        {
            public int languageId { get; }
            public string compilerOptions { get; }

            public LanguageOptions(int languageId) : this(languageId, "")
            {
            }

            public LanguageOptions(int languageId, string compilerOptions)
            {
                this.languageId = languageId;
                this.compilerOptions = compilerOptions;
            }
        }

        public static Dictionary<Language, LanguageOptions> LanguageDict = new Dictionary<Language, LanguageOptions>()
        {
            {Language.C, new LanguageOptions(50, "-DONLINE_JUDGE --static -O2 --std=c11")},
            {Language.CSharp, new LanguageOptions(51)},
            {Language.Cpp, new LanguageOptions(54, "-DONLINE_JUDGE --static -O2 --std=c++17")},
            {Language.Go, new LanguageOptions(60)},
            {Language.Haskell, new LanguageOptions(61)},
            {Language.Java13, new LanguageOptions(62, "-J-Xms32m -J-Xmx256m")},
            {Language.JavaScript, new LanguageOptions(63)},
            {Language.Lua, new LanguageOptions(64)},
            {Language.Php, new LanguageOptions(68)},
            {Language.Python3, new LanguageOptions(71)},
            {Language.Ruby, new LanguageOptions(72)},
            {Language.Rust, new LanguageOptions(73)},
            {Language.TypeScript, new LanguageOptions(74)}
        };
    }

    internal class RunnerOptions
    {
        [JsonProperty("source_code")] public string SourceCode { get; set; }
        [JsonProperty("language_id")] public int LanguageId { get; set; }
        [JsonProperty("compiler_options")] public string CompilerOptions { get; set; }
        [JsonProperty("stdin")] public string Stdin { get; set; }
        [JsonProperty("expected_output")] public string ExpectedOutput { get; set; }
        [JsonProperty("cpu_time_limit")] public float CpuTimeLimit { get; set; }
        [JsonProperty("memory_limit")] public float MemoryLimit { get; set; }
    }

    internal class RunnerResponse
    {
        public class RunnerStatus
        {
            [JsonProperty("id")] public Verdict Id { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
        }

        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("compile_output")] public string CompileOutput { get; set; }
        [JsonProperty("stdout")] public string Stdout { get; set; }
        [JsonProperty("stderr")] public string Stderr { get; set; }
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("memory")] public float? Memory { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("status")] public RunnerStatus Status { get; set; }
    }

    public interface ISubmissionRunner
    {
        public string RunInBackground(int submissionId);
        public Task RunAsync(int submissionId);
    }

    public class SubmissionRunner : ISubmissionRunner
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubmissionRunner> _logger;
        private readonly IHttpClientFactory _factory;

        public SubmissionRunner(ApplicationDbContext context,
            ILogger<SubmissionRunner> logger, IHttpClientFactory factory)
        {
            _context = context;
            _logger = logger;
            _factory = factory;
        }

        public string RunInBackground(int submissionId)
        {
            return BackgroundJob.Enqueue(() => RunAsync(submissionId));
        }

        public async Task RunAsync(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission is null)
            {
                throw new ArgumentException("Invalid submission ID");
            }

            await _context.Entry(submission).Reference(s => s.Problem).LoadAsync();

            submission.Verdict = Verdict.InQueue;
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();

            // TODO: implement Judge0 API consumption
            foreach (var sampleCase in submission.Problem.SampleCases)
            {
                if (!await RunSingleCaseAsync(submission, sampleCase, -1, true))
                {
                    return;
                }
            }

            foreach (var (testCase, index) in submission.Problem.TestCases.Select((tc, i) => (tc, i)))
            {
                if (!await RunSingleCaseAsync(submission, testCase, index, false))
                {
                    return;
                }
            }

            submission.Verdict = Verdict.Accepted;
            submission.JudgedAt = DateTime.Now;
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RunSingleCaseAsync(Submission submission, TestCase testCase, int index, bool inline)
        {
            string input, output;
            if (inline)
            {
                input = testCase.Input;
                output = testCase.Output;
            }
            else
            {
                throw new NotImplementedException("Non-inline test cases not implemented.");
            }

            using var client = _factory.CreateClient();
            var options = new RunnerOptions()
            {
                SourceCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(submission.Program.Code)),
                LanguageId = Languages.LanguageDict[submission.Program.Language.GetValueOrDefault()].languageId,
                CompilerOptions = Languages.LanguageDict[submission.Program.Language.GetValueOrDefault()]
                    .compilerOptions,
                Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(input)),
                ExpectedOutput = Convert.ToBase64String(Encoding.UTF8.GetBytes(output)),
                CpuTimeLimit = (float) submission.Problem.TimeLimit / 1000,
                MemoryLimit = (float) submission.Problem.MemoryLimit
            };

            var json = new StringContent(JsonConvert.SerializeObject(options), Encoding.UTF8,
                MediaTypeNames.Application.Json);
            var data =
                await client.PostAsync("http://localhost:3000/submissions?base64_encoded=true&wait=true", json);

            var response = JsonConvert.DeserializeObject<RunnerResponse>(await data.Content.ReadAsStringAsync());
            submission.Verdict = response.Status == null
                ? Verdict.Failed
                : (response.Status.Id == Verdict.Accepted ? Verdict.Running : response.Status.Id);
            submission.LastTestCase = index;
            submission.JudgedAt = DateTime.Now;
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();

            return submission.Verdict == Verdict.Running;
        }
    }
}