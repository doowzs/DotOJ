using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;
using Worker.Runners.JudgeSubmission.LanguageTypes;

namespace Worker.Runners.JudgeSubmission.ProblemTypes
{
    public class PlainRunner
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;
        protected readonly IServiceProvider Provider;

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public PlainRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Provider = provider;
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            LanguageTypes.Base.Runner runner;
            switch (Submission.Program.Language)
            {
                case Language.C:
                    runner = new CRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Cpp:
                    runner = new CppRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Java:
                    runner = new JavaRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Python:
                    runner = new PythonRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Golang:
                    runner = new GolangRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Rust:
                    runner = new RustRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.CSharp:
                    runner = new CSharpRunner(Contest, Problem, Submission, Provider);
                    break;
                case Language.Haskell:
                    runner = new HaskellRunner(Contest, Problem, Submission, Provider);
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