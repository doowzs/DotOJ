using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public sealed class PracticeRunner : ContestRunnerBase
    {
        public PracticeRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
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
                    FailedOn = new List<int>(
                        new int[]{0}
                    ),
                    Score = 0,
                    Message = run.Message
                });
            }

            return Task.FromResult<JudgeResult>(null);
        }
    }
}