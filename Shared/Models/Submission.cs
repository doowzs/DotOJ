using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Shared.Configs;
using Shared.Generics;

namespace Shared.Models
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

        /**
         * If submitted before contest begins, this value shall be set to true to hide the submission.
         * We add this extra attribute because including all reference to contest will cause performance issues.
         */
        [DefaultValue(false)]
        public bool Hidden { get; set; }

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
        public string JudgedBy { get; set; }
        public DateTime? JudgedAt { get; set; }
        
        // Two version numbers are used to avoid duplicate message in MQ.
        public int RequestVersion { get; set; }
        public int CompleteVersion { get; set; }

        #endregion

        public void ResetVerdictFields()
        {
            Time = Memory = null;
            FailedOn = null;
            Score = Progress = null;
            Message = null;
            JudgedBy = null;
            JudgedAt = null;
        }

        #region Submission Info Comments String

        public string GetInfoCommentsString(IOptions<ApplicationConfig> options, bool omitCommentSign = false)
        {
            var comment = omitCommentSign ? "" : Program.GetSourceFileCommentSign();
            var builder = new StringBuilder();
            builder.AppendLine(comment + $"Submission  #{Id}");
            builder.AppendLine(comment + $"User ID:    {UserId}");
            if (User is not null)
            {
                builder.AppendLine(comment + $"Contestant: {User.ContestantId} {User.ContestantName}");
            }

            if (Problem is not null)
            {
                builder.AppendLine(comment + $"Problem:    No.{ProblemId} {Problem.Title}");
            }
            else
            {
                builder.AppendLine(comment + $"Problem:    No.{ProblemId}");
            }

            builder.AppendLine(comment + $"Verdict:    {Verdict} Score={Score ?? 0} Time={Time ?? 0} Memory={Memory ?? 0}");
            builder.AppendLine(comment + $"Submitted:  {CreatedAt:yyyy-MM-dd HH:mm:ss} (UTC Time)");
            if (JudgedAt.HasValue)
            {
                builder.AppendLine(comment + $"Judged:     {JudgedAt:yyyy-MM-dd HH:mm:ss} by {JudgedBy}");
            }

            builder.AppendLine(comment + $"Links:");
            builder.AppendLine(comment + $"  1. {options.Value.Host}/submission/{Id}");
            if (Problem is not null)
            {
                builder.AppendLine(comment + $"  2. {options.Value.Host}/contest/{Problem.ContestId}/problem/{ProblemId}");
                if (User is not null)
                {
                    builder.AppendLine(comment + $"  3. {options.Value.Host}/contest/{Problem.ContestId}/submissions?problem={ProblemId}&contestantId={User.ContestantId}");
                }
            }

            builder.AppendLine();
            return builder.ToString();
        }

        #endregion
    }
}