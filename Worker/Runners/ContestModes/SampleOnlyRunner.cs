using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.ContestModes
{
    public class SampleOnlyRunner : ContestRunnerBase
    {
        public SampleOnlyRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            BeforeTestGroupDelegate = BeforeTestGroupImpl;
            OnRunFailedDelegate = PracticeRunner.OnRunFailedImpl;
        }

        public static Task<Result> BeforeTestGroupImpl
            (Contest contest, Problem problem, Submission submission, bool inline)
        {
            if (inline == false)
            {
                return OneShotRunner.BeforeStartImpl(contest, problem, submission);
            }

            return Task.FromResult<Result>(null);
        }
    }
}