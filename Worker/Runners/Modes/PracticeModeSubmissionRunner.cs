using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public class PracticeModeSubmissionRunner : ModeSubmissionRunnerBase<PracticeModeSubmissionRunner>
    {
        public PracticeModeSubmissionRunner(IServiceProvider provider) : base(provider)
        {
            AfterPollingRunsDelegate = AfterPollingRunsDelegateImpl;
        }

        private Task<Result> AfterPollingRunsDelegateImpl(Submission submission, Problem problem, List<Run> runs)
        {
            var failed = runs.FirstOrDefault(r => r.Verdict > Verdict.Accepted);
            if (failed != null)
            {
                submission.Verdict = submission.Verdict == Verdict.Running ? failed.Verdict : submission.Verdict;
                // No need to save changes here, will be saved by delegate caller before next loop.
            }

            if (runs.Any(r => r.Verdict <= Verdict.Running))
            {
                return null; // Not all runs have finished, ask for another loop.
            }

            int count = 0, total = runs.Count;
            float time = 0, memory = 0;

            foreach (var run in runs)
            {
                if (run.Index > 0 && run.Verdict == Verdict.Accepted)
                {
                    ++count;
                }

                if (run.Time.HasValue)
                {
                    time = Math.Max(time, run.Time.Value);
                }

                if (run.Memory.HasValue)
                {
                    memory = Math.Max(memory, run.Memory.Value);
                }
            }

            var language = submission.Program.Language ?? Language.C;
            var factor = RunnerLanguageOptions.LanguageOptionsDict[language].timeFactor;
            return Task.FromResult(new Result
            {
                // If there was any failure, submission's verdict will be changed from Running.
                Verdict = submission.Verdict == Verdict.Running ? Verdict.Accepted : submission.Verdict,
                Time = (int) Math.Min(time * 1000, problem.TimeLimit * factor),
                Memory = (int) Math.Min(memory, problem.MemoryLimit),
                FailedOn = failed?.Index,
                Score = count / total,
                Message = ""
            });
        }
    }
}