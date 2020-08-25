using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Judge1.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Judge1.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestNotice> ContestNotices { get; set; }
        public DbSet<ContestRegistration> ContestRegistrations { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Hack> Hacks { get; set; }
        public DbSet<Test> Tests { get; set; }

        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Setup composite key for ContestRegistration.
            builder.Entity<ContestRegistration>()
                .HasKey(ar => new {ar.UserId, ar.ContestId});
            
            // Remove multiple cascade paths for Hack and Test.
            builder.Entity<Hack>()
                .HasOne(h => h.User)
                .WithMany(u => u.Hacks)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Test>()
                .HasOne(h => h.User)
                .WithMany(u => u.Tests)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            InjectCreatedAndUpdatedTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InjectCreatedAndUpdatedTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void InjectCreatedAndUpdatedTimestamps()
        {
            var now = DateTime.Now.ToUniversalTime();
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ModelWithTimestamps 
                            && (e.State == EntityState.Added || e.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((ModelWithTimestamps) entry.Entity).CreatedAt = now;
                }
                ((ModelWithTimestamps) entry.Entity).UpdatedAt = now;
            }
        }
    }
}
