using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Generics;
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
        public string JudgedBy { get; set; }
        public DateTime? JudgedAt { get; set; }

        #endregion
    }
}