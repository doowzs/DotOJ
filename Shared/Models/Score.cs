using System;
using Shared.Generics;

namespace Shared.Models
{
    public class Score
    {

        public int? id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int? SubmissionId { get; set; }
        public Submission Submission { get; set; }

        [Required] public int? Score { get; set; }

        public string Comments { get; set; }
   }
}
