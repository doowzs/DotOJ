using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public class PracticeModeSubmissionRunner : ModeSubmissionRunnerBase<PracticeModeSubmissionRunner>
    {
        public PracticeModeSubmissionRunner(IServiceProvider provider) : base(provider)
        {
        }

        public override Task<Result> OnRunFailed(Submission submission, Problem problem, Run run)
        {
            if (run.Index == 0 || run.Verdict == Verdict.CompilationError || run.Verdict == Verdict.InternalError)
            {
                return Task.FromResult(new Result
                {
                    Verdict = run.Verdict,
                    Time = run.Time.HasValue ? (int) Math.Min(run.Time.Value * 1000, run.TimeLimit) : (int?) null,
                    Memory = run.Memory.HasValue ? (int) Math.Min(run.Memory.Value, problem.MemoryLimit) : (int?) null,
                    FailedOn = run.Index,
                    Score = (run.Index - 1) / problem.TestCases.Count,
                    Message = run.Message
                });
            }

            return null;
        }
    }
}