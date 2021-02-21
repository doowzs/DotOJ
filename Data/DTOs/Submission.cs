using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Data.Configs;
using Data.Generics;
using Data.Models;
using Microsoft.Extensions.Options;

namespace Data.DTOs
{
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
        public bool HasMessage { get; }
        public bool Viewable { get; }

        public SubmissionInfoDto(Submission submission, bool viewable = false) : base(submission)
        {
            var count = submission.Program.Code.Length;
            var padding = submission.Program.Code.Substring(count - 2, 2).Count(c => c == '=');

            Id = submission.Id;
            UserId = submission.UserId;
            ContestantId = submission.User.ContestantId;
            ContestantName = submission.User.ContestantName;
            ProblemId = submission.ProblemId;
            Language = submission.Program.Language.GetValueOrDefault();
            CodeBytes = 3 * count / 4 - padding;
            Verdict = submission.Verdict;
            Time = submission.Time;
            Memory = submission.Memory;
            FailedOn = submission.FailedOn;
            Score = submission.Score;
            Progress = submission.Progress;
            HasMessage = !string.IsNullOrEmpty(submission.Message);
            Viewable = viewable;
        }
    }

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
        public string Comments { get; }
        public string JudgedBy { get; }
        public DateTime? JudgedAt { get; }

        public SubmissionViewDto(Submission submission, IOptions<ApplicationConfig> options) : base(submission)
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
            FailedOn = submission.FailedOn > 0 ? 1 : 0; // Hide this info from users
            Score = submission.Score;
            Progress = submission.Progress;
            Message = submission.Message;
            Comments = submission.GetInfoCommentsString(options, true);
            JudgedBy = submission.JudgedBy;
            JudgedAt = submission.JudgedAt;
        }
    }

    public class SubmissionCreateDto
    {
        // UserID comes from UserManager.
        [Required] public int? ProblemId { get; set; }
        [Required] public Program Program { get; set; }

        public SubmissionCreateDto()
        {
        }
    }

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
        public string JudgedBy { get; }
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
            JudgedBy = submission.JudgedBy;
            JudgedAt = submission.JudgedAt;
            CreatedAt = submission.CreatedAt;
        }
    }
}