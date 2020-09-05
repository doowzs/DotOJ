using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.ContestModes
{
    public class PracticeRunner : ContestRunnerBase
    {
        public PracticeRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            OnRunFailedDelegate = OnRunFailedImpl;
        }

        public static Task<JudgeResult> OnRunFailedImpl(Contest contest, Problem problem, Submission submission, Run run)
        {
            if (run.Index == 0 || run.Verdict == Verdict.CompilationError || run.Verdict == Verdict.InternalError)
            {
                return Task.FromResult(new JudgeResult
                {
                    Verdict = run.Verdict,
                    Time = null,
                    Memory = null,
                    FailedOn = 0,
                    Score = 0,
                    Message = run.Message
                });
            }

            return Task.FromResult<JudgeResult>(null);
        }
    }
}