using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Judge1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [InverseProperty("User")]
        public List<Submission> Submissions { get; set; }
        public List<Hack> Hacks { get; set; }
    }
}
