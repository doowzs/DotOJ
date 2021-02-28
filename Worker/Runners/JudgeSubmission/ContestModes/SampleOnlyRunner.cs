using System;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public sealed class SampleOnlyRunner : ContestRunnerBase
    {
        public SampleOnlyRunner(Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            BeforeTestGroupDelegate = BeforeTestGroupImpl;
            OnRunFailedDelegate = PracticeRunner.OnRunFailedImpl;
        }

        public static Task<JudgeResult> BeforeTestGroupImpl
            (Contest contest, Problem problem, Submission submission, bool inline)
        {
            if (inline == false)
            {
                return OneShotRunner.BeforeStartImpl(contest, problem, submission);
            }

            return Task.FromResult<JudgeResult>(null);
        }
    }
}