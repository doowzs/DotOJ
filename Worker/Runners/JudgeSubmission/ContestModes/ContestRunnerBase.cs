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
        protected readonly Box Box;
        protected readonly IServiceProvider Provider;

        protected Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        protected Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        protected Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public ContestRunnerBase
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Box = box;
            Provider = provider;
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            PlainRunner runner = Problem.Type switch
            {
                ProblemType.Ordinary => Problem.HasSpecialJudge switch
                {
                    true => new SpecialJudgeRunner(Contest, Problem, Submission, Box, Provider),
                    false => new PlainRunner(Contest, Problem, Submission, Box, Provider)
                },
                ProblemType.TestKitLab => new TestKitLabRunner(Contest, Problem, Submission, Box, Provider),
                _ => throw new ArgumentOutOfRangeException()
            };
            runner.BeforeStartDelegate = BeforeStartDelegate;
            runner.BeforeTestGroupDelegate = BeforeTestGroupDelegate;
            runner.OnRunFailedDelegate = OnRunFailedDelegate;
            return await runner.RunSubmissionAsync();
        }
    }
}