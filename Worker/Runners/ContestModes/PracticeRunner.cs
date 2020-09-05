using System;
using System.Threading.Tasks;
using Data;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;
using Worker.Runners.ProblemTypes;

namespace Worker.Runners.ContestModes
{
    public class PracticeRunner : ISubmissionRunner
    {
        private readonly Contest _contest;
        private readonly Problem _problem;
        private readonly Submission _submission;
        private readonly IServiceProvider _provider;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<PracticeRunner> _logger;

        public PracticeRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            _contest = contest;
            _problem = problem;
            _submission = submission;
            _provider = provider;

            _context = provider.GetRequiredService<ApplicationDbContext>();
            _logger = provider.GetRequiredService<ILogger<PracticeRunner>>();
        }

        public async Task<Result> RunSubmissionAsync()
        {
            TestCaseRunnerBase runner;
            if (_problem.HasSpecialJudge)
            {
                throw new NotImplementedException();
            }
            else
            {
                runner = new PlainRunnerBase(_contest, _problem, _submission, _provider);
            }

            runner.OnRunFailedDelegate = OnRunFailedImpl;
            return await runner.RunSubmissionAsync();
        }

        public Task<Result> OnRunFailedImpl(Contest contest, Submission submission, Problem problem, Run run)
        {
            if (run.Index == 0 || run.Verdict == Verdict.CompilationError || run.Verdict == Verdict.InternalError)
            {
                return Task.FromResult(new Result
                {
                    Verdict = run.Verdict,
                    Time = null,
                    Memory = null,
                    FailedOn = 0,
                    Score = 0,
                    Message = run.Message
                });
            }

            return Task.FromResult<Result>(null);
        }
    }
}