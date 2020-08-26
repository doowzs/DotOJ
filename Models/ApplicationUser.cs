using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Judge1.Models
{
    [NotMapped]
    public class ApplicationRoles
    {
        public const string Administrator = "Administrator";
        public const string UserManager = "UserManager";
        public const string ContestManager = "ContestManager";
        public const string JudgeResultManager = "JudgeResultManager";
        
        public static IEnumerable<string> RoleList = new List<string>()
        {
            Administrator,
            UserManager,
            ContestManager,
            JudgeResultManager
        };
    }
    
    public class ApplicationUser : IdentityUser
    {
        [Required] public string ContestantId { get; set; }
        [Required] public string ContestantName { get; set; }
        
        public List<Submission> Submissions { get; set; }
        public List<Hack> Hacks { get; set; }
        public List<Test> Tests { get; set; }
    }
}
