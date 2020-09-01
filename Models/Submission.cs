using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace Judge1.Models
{
    public class Submission : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Relationships
        
        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProblemId { get; set; }
        public Problem Problem { get; set; }

        #endregion

        #region Submission Content

        [NotMapped] public Program Program { get; set; }
        [Required, Column("program", TypeName = "text")]
        public string ProgramSerialized
        {
            get => JsonConvert.SerializeObject(Program);
            set => Program = string.IsNullOrEmpty(value)
                ? new Program()
                : JsonConvert.DeserializeObject<Program>(value);
        }

        #endregion

        #region Submission Verdict

        public Verdict Verdict { get; set; }
        public int FailedOn { get; set; }
        public int Score { get; set; }
        public DateTime JudgedAt { get; set; }

        #endregion
    }

    public class Hack : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Relationships

        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }

        #endregion

        #region Hacking Content and Verdict

        [Required, Column(TypeName = "text")] public string Input { get; set; }
        
        public bool? IsValid { get; set; }
        public DateTime ValidatedAt { get; set; }
        
        public bool? IsSuccessful { get; set; }
        public DateTime JudgedAt { get; set; }

        #endregion
    }

    public class Test : ModelWithTimestamps
    {
        public int Id { get; set; }
        
        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int ProblemId { get; set; }
        public Problem Problem { get; set; }
        
        [NotMapped] public Program Program { get; set; }
        [Required, Column("program", TypeName = "text")]
        public string ProgramSerialized
        {
            get => JsonConvert.ToString(Program);
            set => Program = string.IsNullOrEmpty(value)
                ? null
                : JsonConvert.DeserializeObject<Program>(value);
        }
        
        [Required, Column(TypeName = "text")] public string Input { get; set; }
        [Column(TypeName = "text")] public string Output { get; set; }

        public Verdict Verdict { get; set; }
        public DateTime JudgedAt { get; set; }
    }

    [NotMapped]
    public class SubmissionInfoDto : DtoWithTimestamps
    {
        public int Id { get; }
        public string UserId { get; }
        public int ProblemId { get; }
        public Language Language { get; }
        public int CodeBytes { get; }
        public Verdict Verdict { get; }
        public int FailedOn { get; }
        public int Score { get; }
        public DateTime JudgedAt { get; }

        public SubmissionInfoDto(Submission submission) : base(submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ProblemId = submission.ProblemId;
            Language = submission.Program.Language.GetValueOrDefault();
            CodeBytes = Encoding.UTF8.GetByteCount(submission.Program.Code);
            Verdict = submission.Verdict;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            JudgedAt = submission.JudgedAt;
        }
    }

    [NotMapped]
    public class SubmissionViewDto : DtoWithTimestamps
    {
        public int Id { get; }
        public string UserId { get; }
        public int? ProblemId { get; }
        public Program Program { get; }
        public Verdict Verdict { get; }
        public int FailedOn { get; }
        public int Score { get; }
        public DateTime JudgedAt { get; }

        public SubmissionViewDto(Submission submission) : base(submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ProblemId = submission.ProblemId;
            Program = submission.Program;
            Verdict = submission.Verdict;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            JudgedAt = submission.JudgedAt;
        }
    }

    [NotMapped]
    public class SubmissionCreateDto
    {
        // UserID comes from UserManager.
        [Required] public int? ProblemId { get; set; }
        [Required] public Program Program { get; set; }

        public SubmissionCreateDto()
        {
        }
    }

    [NotMapped]
    public class SubmissionEditDto
    {
        public int Id { get; }
        public string UserId { get; }
        public int ProblemId { get; }
        public Program Program { get; }
        [Required] public Verdict? Verdict { get; set; }

        public SubmissionEditDto()
        {
        }

        public SubmissionEditDto(Submission submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ProblemId = submission.ProblemId;
            Program = submission.Program;
            Verdict = submission.Verdict;
        }
    }
}