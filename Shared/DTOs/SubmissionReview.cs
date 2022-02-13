using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Shared.Models;

namespace Shared.DTOs
{
    public class SubmissionReviewInfoDto
    {
        public string ContestantId { get; set; }
        [Required] public int? Score { get; set; }
        
        [Required] public string TimeComplexity { get; set; }
        
        [Required] public string SpaceComplexity { get; set; }
        
        [Required] public string CodeSpecification { get; set; }
        
        public string Comments { get; set; }
        public SubmissionViewDto Submission { get; set; }

        public SubmissionReviewInfoDto(int? score, string timeComplexity, string spaceComplexity, string codeSpecification, string comments, SubmissionViewDto submissionDto, string contestantId)
        {
            ContestantId = contestantId;
            Score = score;
            TimeComplexity = timeComplexity;
            SpaceComplexity = spaceComplexity;
            CodeSpecification = codeSpecification;
            Comments = comments;
            Submission = submissionDto;
        }
    }
    public class SubmissionReviewCreateDto
    {
      
        [Required] public List<int> SubmissionId { get; set; }
        [Required] public int? ProblemId { get; set; }
        [Required] public List<int> Score { get; set; }

        [Required] public List<string> TimeComplexity { get; set; }
        
        [Required] public List<string> SpaceComplexity { get; set; }
        
        [Required] public List<string> CodeSpecification { get; set; }
        
        public List<string> Comments { get; set; }

        public SubmissionReviewCreateDto()
        {
        }
    }
}