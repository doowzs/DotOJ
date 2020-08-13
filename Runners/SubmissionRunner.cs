using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Judge1.Data;
using Judge1.Models;
using Microsoft.Extensions.Logging;

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
            {Language.C, new LanguageOptions(50, "-DONLINE_JUDGE --static -O2")},
            {Language.C11, new LanguageOptions(50, "-DONLINE_JUDGE --static -O2 --std=c11")},
            {Language.CSharp, new LanguageOptions(51)},
            {Language.Cpp, new LanguageOptions(54, "-DONLINE_JUDGE --static -O2")},
            {Language.Cpp11, new LanguageOptions(54, "-DONLINE_JUDGE --static -O2 --std=c++11")},
            {Language.Cpp17, new LanguageOptions(54, "-DONLINE_JUDGE --static -O2 --std=c++17")},
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
    
    public interface ISubmissionRunner
    {
        public string RunInBackground(int submissionId);
        public Task RunAsync(int submissionId);
    }

    public class SubmissionRunner : ISubmissionRunner
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubmissionRunner> _logger;

        public SubmissionRunner(ApplicationDbContext context, ILogger<SubmissionRunner> logger)
        {
            _context = context;
            _logger = logger;
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
            
        }
    }
}