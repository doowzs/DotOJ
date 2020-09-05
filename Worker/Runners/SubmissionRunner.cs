using System;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification;
using Worker.Models;
using Worker.Runners.ContestModes;

namespace Worker.Runners
{
    public interface ISubmissionRunner
    {
        public Task<Result> RunSubmissionAsync();
    }

    public sealed class SubmissionRunner : ISubmissionRunner
    {
        private readonly Contest _contest;
        private readonly Problem _problem;
        private readonly Submission _submission;

        private readonly ApplicationDbContext _context;
        private readonly INotificationBroadcaster _broadcaster;
        private readonly IOptions<JudgingConfig> _options;
        private readonly ILogger<SubmissionRunner> _logger;
        private readonly IServiceProvider _provider;

        public SubmissionRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            _contest = contest;
            _problem = problem;
            _submission = submission;

            _context = provider.GetRequiredService<ApplicationDbContext>();
            _broadcaster = provider.GetRequiredService<INotificationBroadcaster>();
            _options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            _logger = provider.GetRequiredService<ILogger<SubmissionRunner>>();
            _provider = provider;
        }

        public async Task<Result> RunSubmissionAsync()
        {
            ContestRunnerBase runner;
            switch (_contest.Mode)
            {
                case ContestMode.Practice:
                    runner = new PracticeRunner(_contest, _problem, _submission, _provider);
                    break;
                case ContestMode.UntilFail:
                    runner = new UntilFailRunner(_contest, _problem, _submission, _provider);
                    break;
                case ContestMode.OneShot:
                    runner = new OneShotRunner(_contest, _problem, _submission, _provider);
                    break;
                case ContestMode.SampleOnly:
                    runner = new SampleOnlyRunner(_contest, _problem, _submission, _provider);
                    break;
                default:
                    throw new Exception($"Unknown contest mode ${_contest.Mode}");
            }

            return await runner.RunSubmissionAsync();
        }
    }
}