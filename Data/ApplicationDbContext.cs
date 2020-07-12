using Judge1.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Judge1.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentNotice> AssignmentNotices { get; set; }
        public DbSet<AssignmentRegistration> AssignmentRegistrations { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Hack> Hacks { get; set; }

        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AssignmentRegistration>()
                .HasKey(ar => new {ar.UserId, ar.AssignmentId});
        }
    }
}
