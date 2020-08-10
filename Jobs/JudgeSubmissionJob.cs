using System.Threading.Tasks;
using Hangfire;
using Judge1.Models;
using Judge1.Services;
using Microsoft.Extensions.Logging;

namespace Judge1.Jobs
{
    public class JudgeSubmissionJob
    {
        private readonly ISubmissionService _service;
        private readonly ILogger<ISubmissionService> _logger;
        private readonly Submission _submission;

        // Use ActivatorUtilities.CreateInstance<T> to create a job instance. See below for details.
        // https://stackoverflow.com/questions/52644507/using-activatorutilities-createinstance
        public JudgeSubmissionJob(ISubmissionService service, ILogger<ISubmissionService> logger, Submission submission)
        {
            _service = service;
            _logger = logger;
            _submission = submission;

            // TODO: change this class with a DI parameters only constructor
            // See https://github.com/HangfireIO/Hangfire/issues/924
            // BackgroundJob.Enqueue(() => Run());
        }

        public async Task Run()
        {
            await _service.UpdateSubmissionVerdictAsync(_submission, Verdict.Accepted, 10);
        }
    }
}