using System;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public sealed class OneShotRunner : ContestRunnerBase
    {
        public OneShotRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            BeforeStartDelegate = BeforeStartImpl;
            OnRunFailedDelegate = PracticeRunner.OnRunFailedImpl;
        }

        public static Task<JudgeResult> BeforeStartImpl(Contest contest, Problem problem, Submission submission)
        {
            if (DateTime.Now.ToUniversalTime() <= contest.EndTime)
            {
                return Task.FromResult(new JudgeResult
                {
                    Verdict = Verdict.Accepted,
                    Time = null,
                    Memory = null,
                    FailedOn = null,
                    Score = 0,
                    Message = ""
                });
            }

            return Task.FromResult<JudgeResult>(null);
        }
    }
}