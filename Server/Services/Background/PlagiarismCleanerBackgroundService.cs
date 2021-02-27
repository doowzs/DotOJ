using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared;
using Shared.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Server.Services.Background
{
    public class PlagiarismCleanerBackgroundService : CronJobService
    {
        private readonly IOptions<ApplicationConfig> _options;

        public PlagiarismCleanerBackgroundService(IServiceProvider provider) : base(provider, "0 * * * *")
        {
            _options = provider.GetRequiredService<IOptions<ApplicationConfig>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = Factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.Now.ToUniversalTime();
            var plagiarisms = await context.Plagiarisms
                .Where(p => !p.Outdated && p.CreatedAt <= now.AddDays(-7))
                .ToListAsync(stoppingToken);
            foreach (var plagiarism in plagiarisms)
            {
                var folder = Path.Combine(_options.Value.DataPath, "plagiarisms", plagiarism.Id.ToString());
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                plagiarism.Outdated = true;
            }
            context.UpdateRange(plagiarisms);
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}