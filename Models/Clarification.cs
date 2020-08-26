using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    public class Clarification : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Relationships
        
        public int ContestId { get; set; }
        public Contest Contest { get; set; }
        
        public int? ProblemId { get; set; }
        public Problem Problem { get; set; }
        
        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        #endregion

        #region Content and Reply

        [Required, Column(TypeName = "text")] public string content { get; set; }
        
        [Column(TypeName = "text")] public string Reply { get; set; }
        public bool IsPublic { get; set; }
        public DateTime RepliedAt { get; set; }
        public string RepliedBy { get; set; }

        #endregion
    }

    [NotMapped]
    public class ClarificationViewDto : DtoWithTimestamps
    {
        public int Id { get; }
        public int ContestId { get; }
        public int? ProblemId { get; }
        public string UserId { get; }
        public string ProblemTitle { get; }
        public string Content { get; }
        public string Reply { get; }
        public DateTime RepliedAt { get; }
        public string RepliedBy { get; }

        public ClarificationViewDto(Clarification clarification) : base(clarification)
        {
            Id = clarification.Id;
            ContestId = clarification.ContestId;
            ProblemId = clarification.ProblemId;
            if (ProblemId != null)
            {
                ProblemTitle = clarification.Problem.Title;
            }

            Content = clarification.content;
            Reply = clarification.Reply;
            RepliedAt = clarification.RepliedAt;
            RepliedBy = clarification.RepliedBy;
        }
    }

    [NotMapped]
    public class ClarificationEditDto
    {
        public int Id { get; set; }
        public int ContestId { get; set; }
        public int? ProblemId { get; set; }
        [Required] public string Content { get; set; }

        public ClarificationEditDto()
        {
        }
    }

    [NotMapped]
    public class ClarificationReplyDto
    {
        [Required] public string Reply { get; set; }
        public bool IsPublic { get; set; }

        public ClarificationReplyDto()
        {
        }
    }
}