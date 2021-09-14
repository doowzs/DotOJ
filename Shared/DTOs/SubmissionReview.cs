using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class SubmissionReviewCreateDto
    {
      
        [Required] public int SubmissionId { get; set; }
        [Required] public int? ProblemId { get; set; }
        [Required] public int? Score { get; set; }
        [Required] public string Comments { get; set; }

        public SubmissionReviewCreateDto()
        {
        }
    }
}