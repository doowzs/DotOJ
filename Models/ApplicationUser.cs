using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Judge1.Models
{
    public static class ApplicationRoles
    {
        public const string ProblemEditor = "ProblemEditor";
        public const string AssignmentManager = "AssignmentManager";
        public const string JudgeManager = "JudgeManager";
        public const string UserManager = "UserManager";
    }
    
    public class ApplicationUser : IdentityUser
    {
        [InverseProperty("User")]
        public List<Submission> Submissions { get; set; }
        public List<Hack> Hacks { get; set; }
    }
}
