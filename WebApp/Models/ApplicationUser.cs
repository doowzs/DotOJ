using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Judge1.Models
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

    [NotMapped]
    public class ApplicationUserInfoDto
    {
        public string Id { get; }
        public string Email { get; }
        public string UserName { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }

        public ApplicationUserInfoDto(ApplicationUser user)
        {
            Id = user.Id;
            Email = user.Email;
            UserName = user.UserName;
            ContestantId = user.ContestantId;
            ContestantName = user.ContestantName;
        }
    }

    [NotMapped]
    public class ApplicationUserEditDto
    {
        public string Id { get; }
        public string Email { get; set; }
        public string UserName { get; }
        [Required] public string ContestantId { get; set; }
        [Required] public string ContestantName { get; set; }
        [Required] public bool? IsAdministrator { get; set; }
        [Required] public bool? IsUserManager { get; set; }
        [Required] public bool? IsContestManager { get; set; }
        [Required] public bool? IsSubmissionManager { get; set; }

        public ApplicationUserEditDto()
        {
        }

        public ApplicationUserEditDto(ApplicationUser user, IList<string> roles)
        {
            Id = user.Id;
            Email = user.Email;
            UserName = user.UserName;
            ContestantId = user.ContestantId;
            ContestantName = user.ContestantName;
            IsAdministrator = roles.Contains(ApplicationRoles.Administrator);
            IsUserManager = roles.Contains(ApplicationRoles.UserManager);
            IsContestManager = roles.Contains(ApplicationRoles.ContestManager);
            IsSubmissionManager = roles.Contains(ApplicationRoles.SubmissionManager);
        }
    }
}