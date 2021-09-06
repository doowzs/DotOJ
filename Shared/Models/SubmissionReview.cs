using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Generics;

namespace Shared.Models
{
    public class SubmissionReview : ModelWithTimestamps
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }

        [Required] public int? Score { get; set; }

        [Column("program", TypeName = "text")]public string Comments { get; set; }
    }
}