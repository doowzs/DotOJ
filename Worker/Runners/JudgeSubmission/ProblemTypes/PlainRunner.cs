using System;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;
using Worker.Runners.JudgeSubmission.LanguageTypes;
using Worker.Runners.JudgeSubmission.LanguageTypes.TestKit;

namespace Worker.Runners.JudgeSubmission.ProblemTypes
{
    public class PlainRunner
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;
        protected readonly Box Box;
        protected readonly IServiceProvider Provider;

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        public PlainRunner(Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Box = box;
            Provider = provider;
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            LanguageTypes.Base.Runner runner;
            switch (Submission.Program.Language)
            {
                case Language.C:
                    runner = new CRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Cpp:
                    runner = new CppRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Java:
                    runner = new JavaRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Python:
                    runner = new PythonRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Golang:
                    runner = new GolangRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Rust:
                    runner = new RustRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.CSharp:
                    runner = new CSharpRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.Haskell:
                    runner = new HaskellRunner(Contest, Problem, Submission, Box, Provider);
                    break;
                case Language.LabArchive:
                    return Problem.Type switch
                    {
                        ProblemType.Ordinary => JudgeResult.UnknownLanguageFailure,
                        ProblemType.TestKitLab => await
                            new TestKitRunner(Contest, Problem, Submission, Box, Provider).RunSubmissionAsync(),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                default:
                    return JudgeResult.UnknownLanguageFailure;
            }

            runner.BeforeStartDelegate = BeforeStartDelegate;
            runner.BeforeTestGroupDelegate = BeforeTestGroupDelegate;
            runner.OnRunFailedDelegate = OnRunFailedDelegate;

            return await runner.RunSubmissionAsync();
        }
    }
}