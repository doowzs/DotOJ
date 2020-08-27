using System;
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
            submission.FailedOn = -1;
            submission.Score = 100;
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
                // TODO: implement file based test cases
                throw new NotImplementedException("Non-inline test cases not implemented.");
            }

            using var client = _factory.CreateClient();
            var json = new StringContent(
                JsonConvert.SerializeObject(new RunnerOptions(submission, input, output)),
                Encoding.UTF8, MediaTypeNames.Application.Json
            );
            // TODO: get backand address from config file
            var data =
                await client.PostAsync("http://localhost:3000/submissions?base64_encoded=true&wait=true", json);

            var response = JsonConvert.DeserializeObject<RunnerResponse>(await data.Content.ReadAsStringAsync());
            submission.Verdict = response.Status == null
                ? Verdict.Failed
                : (response.Status.Id == Verdict.Accepted ? Verdict.Running : response.Status.Id);
            if (response.Status == null || response.Status.Id != Verdict.Accepted)
            {
                submission.FailedOn = index;
                submission.Score = 0;
            }
            submission.JudgedAt = DateTime.Now;
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();

            return submission.Verdict == Verdict.Running;
        }
    }
}