using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace Data.Models
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
        public int? Time { get; set; }
        public int? Memory { get; set; }
        public int? FailedOn { get; set; }
        public int? Score { get; set; }
        public int? Progress { get; set; }
        public string Message { get; set; }
        public DateTime? JudgedAt { get; set; }

        #endregion
    }

    [NotMapped]
    public class SubmissionInfoDto : DtoWithTimestamps
    {
        public int Id { get; }
        public string UserId { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ProblemId { get; }
        public Language Language { get; }
        public int CodeBytes { get; }
        public Verdict Verdict { get; }
        public int? Time { get; }
        public int? Memory { get; }
        public int? FailedOn { get; }
        public int? Score { get; }
        public int? Progress { get; }
        public DateTime? JudgedAt { get; }

        public SubmissionInfoDto(Submission submission) : base(submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ContestantId = submission.User.ContestantId;
            ContestantName = submission.User.ContestantName;
            ProblemId = submission.ProblemId;
            Language = submission.Program.Language.GetValueOrDefault();
            CodeBytes = Encoding.UTF8.GetByteCount(submission.Program.Code);
            Verdict = submission.Verdict;
            Time = submission.Time;
            Memory = submission.Memory;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            Progress = submission.Progress;
            JudgedAt = submission.JudgedAt;
        }
    }

    [NotMapped]
    public class SubmissionViewDto : DtoWithTimestamps
    {
        public int Id { get; }
        public string UserId { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ProblemId { get; }
        public Program Program { get; }
        public Verdict? Verdict { get; }
        public int? Time { get; }
        public int? Memory { get; }
        public int? FailedOn { get; }
        public int? Score { get; }
        public int? Progress { get; }
        public string Message { get; }
        public DateTime? JudgedAt { get; }

        public SubmissionViewDto(Submission submission) : base(submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ContestantId = submission.User.ContestantId;
            ContestantName = submission.User.ContestantName;
            ProblemId = submission.ProblemId;
            Program = submission.Program;
            Verdict = submission.Verdict;
            Time = submission.Time;
            Memory = submission.Memory;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            Progress = submission.Progress;
            Message = submission.Message;
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
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ProblemId { get; }
        public Program Program { get; }
        [Required] public Verdict? Verdict { get; set; }
        public int? Time { get; }
        public int? Memory { get; }
        public int? FailedOn { get; }
        public int? Score { get; }
        [Required] public string Message { get; set; }
        public DateTime? JudgedAt { get; }
        public DateTime CreatedAt { get; }

        public SubmissionEditDto()
        {
        }

        public SubmissionEditDto(Submission submission)
        {
            Id = submission.Id;
            UserId = submission.UserId;
            ContestantId = submission.User.ContestantId;
            ContestantName = submission.User.ContestantName;
            ProblemId = submission.ProblemId;
            Program = submission.Program;
            Verdict = submission.Verdict;
            Time = submission.Time;
            Memory = submission.Memory;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            Message = submission.Message;
            JudgedAt = submission.JudgedAt;
            CreatedAt = submission.CreatedAt;
        }
    }
}