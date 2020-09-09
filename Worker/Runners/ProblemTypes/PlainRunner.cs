using System;
using System.Threading.Tasks;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;
using Worker.Runners.LanguageTypes;

namespace Worker.Runners.ProblemTypes
{
    public class PlainRunner
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;
        protected readonly IServiceProvider Provider;
        protected ILogger Logger;

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public PlainRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Provider = provider;

            Logger = provider.GetRequiredService<ILogger<PlainRunner>>();
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            LanguageRunnerBase runner;
            switch (Submission.Program.Language)
            {
                case Language.C:
                    runner = new CRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Cpp:
                    runner = new CppRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Python:
                    runner = new Py3Runner(Contest, Problem, Submission, Provider);
                    break;
                default:
                    throw new Exception($"Invalid language Submission={Submission.Id}" +
                                        $" Language={Submission.Program.Language}.");
            }

            runner.BeforeStartDelegate = BeforeStartDelegate;
            runner.BeforeTestGroupDelegate = BeforeTestGroupDelegate;
            runner.OnRunFailedDelegate = OnRunFailedDelegate;

            return await runner.RunSubmissionAsync();
        }
    }
}