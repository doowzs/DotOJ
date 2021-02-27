using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models
{
    [NotMapped]
    public class ApplicationRoles
    {
        public const string Administrator = "Administrator";
        public const string UserManager = "UserManager";
        public const string ContestManager = "ContestManager";
        public const string SubmissionManager = "SubmissionManager";

        public static IEnumerable<string> RoleList = new List<string>()
        {
            Administrator,
            UserManager,
            ContestManager,
            SubmissionManager
        };
    }

    public class ApplicationUser : IdentityUser
    {
        [Required, ProtectedPersonalData] public string ContestantId { get; set; }
        [Required] public string ContestantName { get; set; }

        public List<Submission> Submissions { get; set; }
    }

    public class CustomUserValidator : IUserValidator<ApplicationUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            if (await manager.Users.AnyAsync(u => u.Id != user.Id && u.ContestantId == user.ContestantId))
            {
                return IdentityResult.Failed(new[]
                {
                    new IdentityError
                    {
                        Code = "DuplicateContestantId",
                        Description = "Contestant ID already taken."
                    }
                });
            }

            return IdentityResult.Success;
        }
    }
}