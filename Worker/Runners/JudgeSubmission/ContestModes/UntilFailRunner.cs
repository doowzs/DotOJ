using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public sealed class UntilFailRunner : ContestRunnerBase
    {
        public UntilFailRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
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
                FailedOn = new List<int>(
                    new int[]{run.Index}
                ),
                Score = 0,
                Message = run.Message
            });
        }
    }
}