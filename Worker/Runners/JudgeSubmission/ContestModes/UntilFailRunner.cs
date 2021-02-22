using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public class UntilFailRunner : ContestRunnerBase
    {
        public UntilFailRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            OnRunFailedDelegate = OnRunFailedImpl;
        }

        public static Task<JudgeResult> OnRunFailedImpl(Contest contest, Problem problem, Submission submission, Run run)
        {
            return Task.FromResult(new JudgeResult
            {
                Verdict = run.Verdict,
                Time = run.Time,
                Memory = run.Memory,
                FailedOn = run.Index,
                Score = 0,
                Message = run.Message
            });
        }
    }
}