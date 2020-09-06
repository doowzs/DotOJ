using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Models;

namespace Data.DTOs
{
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

    public class ApplicationUserEditDto
    {
        public string Id { get; }
        public string Email { get; set; }
        public string UserName { get; }
        public string Password { get; set; }
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
            Password = null;
            ContestantId = user.ContestantId;
            ContestantName = user.ContestantName;
            IsAdministrator = roles.Contains(ApplicationRoles.Administrator);
            IsUserManager = roles.Contains(ApplicationRoles.UserManager);
            IsContestManager = roles.Contains(ApplicationRoles.ContestManager);
            IsSubmissionManager = roles.Contains(ApplicationRoles.SubmissionManager);
        }
    }
}