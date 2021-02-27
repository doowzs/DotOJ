using System;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
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
            if (run.Inline || run.Verdict == Verdict.CompilationError)
            {
                return Task.FromResult(new JudgeResult
                {
                    Verdict = run.Verdict,
                    Time = run.Time,
                    Memory = run.Memory,
                    FailedOn = 0,
                    Score = 0,
                    Message = run.Message
                });
            }

            return Task.FromResult<JudgeResult>(null);
        }
    }
}