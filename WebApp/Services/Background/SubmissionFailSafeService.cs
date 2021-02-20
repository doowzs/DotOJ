using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Services.Background
{
    public class SubmissionFailSafeService : CronJobService
    {
        private readonly ApplicationDbContext _context;

        public SubmissionFailSafeService(IServiceProvider provider) : base("* * * * *")
        {
            _context = provider.GetRequiredService<ApplicationDbContext>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var submissions = await _context.Submissions
                .Where(s => s.JudgedAt == null && s.UpdatedAt < DateTime.Now.ToUniversalTime().AddMinutes(-30))
                .ToListAsync(stoppingToken);
            var message = $"*** Killed due to timeout of 30 minutes. ***";
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            
            foreach (var submission in submissions)
            {
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = null;
                submission.Score = 0;
                submission.Message = encoded;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
            }

            _context.UpdateRange(submissions);
            await _context.SaveChangesAsync(stoppingToken);
        }
    }
}