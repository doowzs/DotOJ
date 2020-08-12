using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Judge1.Data;
using Judge1.Models;
using Microsoft.Extensions.Logging;

namespace Judge1.Runners
{
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

            // TODO: implement Judge0 API consumption
            foreach (var sampleCase in submission.Problem.SampleCases)
            {
                submission.Verdict = Verdict.Running;
                submission.LastTestCase = -1;
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();
            }

            foreach (var (testCase, index) in submission.Problem.TestCases.Select((tc, i) => (tc, i)))
            {
                submission.Verdict = Verdict.Running;
                submission.LastTestCase = index;
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();
            }

            submission.Verdict = Verdict.Accepted;
            submission.JudgedAt = DateTime.Now;
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();
        }
    }
}