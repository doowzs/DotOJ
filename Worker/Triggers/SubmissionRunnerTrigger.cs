using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Data;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Worker.Runners;

namespace Worker.Triggers
{
    public sealed class SubmissionRunnerTrigger : TriggerBase<SubmissionRunnerTrigger>
    {
        private readonly SubmissionRunner _runner;
        
        public SubmissionRunnerTrigger(IServiceProvider provider) : base(provider)
        {
            _runner = provider.GetRequiredService<SubmissionRunner>();
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

            Submission submission;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
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
                    submission.JudgedBy = Options.Value.Instance.Name;
                    Context.Update(submission);
                    await Context.SaveChangesAsync();
                }
            }

            if (submission != null)
            {
                await _runner.RunSubmissionAsync(submission);
            }
        }
    }
}