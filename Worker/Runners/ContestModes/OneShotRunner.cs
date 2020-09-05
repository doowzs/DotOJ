using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.ContestModes
{
    public class OneShotRunner : ContestRunnerBase
    {
        public OneShotRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            BeforeStartDelegate = BeforeStartImpl;
            OnRunFailedDelegate = PracticeRunner.OnRunFailedImpl;
        }

        public static Task<Result> BeforeStartImpl(Contest contest, Problem problem, Submission submission)
        {
            if (DateTime.Now.ToUniversalTime() <= contest.EndTime)
            {
                return Task.FromResult(new Result
                {
                    Verdict = Verdict.Accepted,
                    Time = null,
                    Memory = null,
                    FailedOn = null,
                    Score = 0,
                    Message = ""
                });
            }

            return Task.FromResult<Result>(null);
        }
    }
}