using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Runners;

namespace Worker.Triggers
{
    public sealed class SubmissionRunnerTrigger : TriggerBase<SubmissionRunnerTrigger>
    {
        private readonly ISubmissionRunner _runner;

        public SubmissionRunnerTrigger(IServiceProvider provider) : base(provider)
        {
            _runner = provider.GetRequiredService<ISubmissionRunner>();
        }

        public override async Task CheckAndRunAsync()
        {
            var now = DateTime.Now.ToUniversalTime();
            var contestIds = await Context.Contests
                .Where(c => c.BeginTime <= now && now <= c.EndTime)
                .Select(c => c.Id)
                .ToListAsync();
            var problemIds = await Context.Problems
                .Where(p => contestIds.Contains(p.ContestId))
                .Select(p => p.Id)
                .ToListAsync();

            // Update all new pending submissions' status.
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var pendingSubmissions = await Context.Submissions
                    .Where(s => s.Verdict == Verdict.Pending).ToListAsync();
                foreach (var pendingSubmission in pendingSubmissions)
                {
                    pendingSubmission.Verdict = Verdict.InQueue;
                }
                Context.UpdateRange(pendingSubmissions);
                await Context.SaveChangesAsync();
                scope.Complete();
            }

            Submission submission;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Choose a submission that has not been judged yet.
                // Submissions of a running contest have a higher priority to be judged.
                var queryable = Context.Submissions.Where(s => string.IsNullOrEmpty(s.JudgedBy)).AsQueryable();
                if (await queryable.AnyAsync(s => problemIds.Contains(s.Id)))
                {
                    submission = await queryable.Where(s => problemIds.Contains(s.Id))
                        .OrderBy(s => s.Id).FirstOrDefaultAsync();
                }
                else
                {
                    submission = await queryable.OrderBy(s => s.Id).FirstOrDefaultAsync();
                }

                if (submission != null)
                {
                    submission.JudgedBy = Options.Value.Name;
                    Context.Update(submission);
                }

                await Context.SaveChangesAsync();
                scope.Complete();
            }

            if (submission != null)
            {
                await _runner.RunSubmissionAsync(submission);
            }
        }
    }
}