using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;
using Worker.Runners.ProblemTypes;

namespace Worker.Runners.ContestModes
{
    public class ContestRunnerBase : ISubmissionRunner
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;
        protected readonly IServiceProvider Provider;

        protected Func<Contest, Problem, Submission, Task<Result>> BeforeStartDelegate = null;
        protected Func<Contest, Problem, Submission, bool, Task<Result>> BeforeTestGroupDelegate = null;
        protected Func<Contest, Problem, Submission, Run, Task<Result>> OnRunFailedDelegate = null;

        public ContestRunnerBase(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Provider = provider;
        }

        public async Task<Result> RunSubmissionAsync()
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