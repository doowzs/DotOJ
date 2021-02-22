using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;
using Worker.Runners.JudgeSubmission.ContestModes;

namespace Worker.Runners.JudgeSubmission
{
    public interface ISubmissionRunner
    {
        public Task<JudgeResult> RunSubmissionAsync();
    }

    public sealed class SubmissionRunner : ISubmissionRunner
    {
        private readonly Contest _contest;
        private readonly Problem _problem;
        private readonly Submission _submission;
        private readonly IServiceProvider _provider;

        public SubmissionRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            _contest = contest;
            _problem = problem;
            _submission = submission;
            _provider = provider;
        }

        public async Task<JudgeResult> RunSubmissionAsync()
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