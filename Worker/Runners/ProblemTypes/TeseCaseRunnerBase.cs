using System;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.ProblemTypes
{
    public class TestCaseRunnerBase : ISubmissionRunner
    {
        public Func<Contest, Submission, Problem, Task<Result>> BeforeStartDelegate = null;
        public Func<Contest, Submission, Problem, bool, Task<Result>> BeforeTestGroupDelegate = null;
        public Func<Contest, Submission, Problem, Run, Task<Result>> OnRunFailedDelegate = null;

        public virtual Task<Result> RunSubmissionAsync()
        {
            return Task.FromResult<Result>(null);
        }
    }
}