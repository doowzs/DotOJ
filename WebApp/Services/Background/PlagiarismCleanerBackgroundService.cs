using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.Services.Background
{
    public class PlagiarismCleanerBackgroundService : CronJobService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IOptions<ApplicationConfig> _options;
        
        public PlagiarismCleanerBackgroundService(IServiceProvider provider) : base("0 * * * *")
        {
            _factory = provider.GetRequiredService<IServiceScopeFactory>();
            _options = provider.GetRequiredService<IOptions<ApplicationConfig>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var plagiarisms = await context.Plagiarisms
                .Where(p => !p.Outdated && p.CreatedAt <= DateTime.Now.ToUniversalTime().AddDays(-7))
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