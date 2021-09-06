using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Generics;
using Shared.Models;

namespace Shared
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<Bulletin> Bulletins { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        public DbSet<SubmissionReview> SubmissionReviews { get; set; }
        public DbSet<Plagiarism> Plagiarisms { get; set; }

        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Setup unique index for ApplicationUser.
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.ContestantId)
                .IsUnique();

            // Setup composite key for Registration.
            builder.Entity<Registration>()
                .HasKey(ar => new { ar.UserId, ar.ContestId });
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
                    ((ModelWithTimestamps)entry.Entity).CreatedAt = now;
                }

                ((ModelWithTimestamps)entry.Entity).UpdatedAt = now;
            }
        }
    }
}