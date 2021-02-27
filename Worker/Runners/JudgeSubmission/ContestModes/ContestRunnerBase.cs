using System;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;
using Worker.Runners.JudgeSubmission.ProblemTypes;

namespace Worker.Runners.JudgeSubmission.ContestModes
{
    public class ContestRunnerBase
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;
        protected readonly IServiceProvider Provider;

        protected Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        protected Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        protected Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public ContestRunnerBase(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Provider = provider;
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            PlainRunner runner;
            if (Problem.HasSpecialJudge)
            {
                runner = new SpecialJudgeRunner(Contest, Problem, Submission, Provider);
            }
            else
            {
                runner = new PlainRunner(Contest, Problem, Submission, Provider);
            }

            runner.BeforeStartDelegate = BeforeStartDelegate;
            runner.BeforeTestGroupDelegate = BeforeTestGroupDelegate;
            runner.OnRunFailedDelegate = OnRunFailedDelegate;
            return await runner.RunSubmissionAsync();
        }
    }
}