using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace Shared.DTOs
{
    public class SubmissionReviewInfoDto
    {
        
        [Required] public int? Score { get; set; }
        public string Comments { get; set; }
        public SubmissionViewDto Submission { get; set; }

        public SubmissionReviewInfoDto(int? score, string comments, SubmissionViewDto submissionDto)
        {
            Score = score;
            Comments = comments;
            Submission = submissionDto;
        }
    }
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