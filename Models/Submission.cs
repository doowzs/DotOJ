using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        
        public int? AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        #endregion

        #region Submission Content

        [NotMapped] public Program Program { get; set; }
        [Required, Column("program", TypeName = "text")]
        public string ProgramSerialized
        {
            get => JsonConvert.ToString(Program);
            set => Program = string.IsNullOrEmpty(value)
                ? null
                : JsonConvert.DeserializeObject<Program>(value);
        }

        #endregion

        #region Submission Verdict

        public Verdict Verdict { get; set; }
        public int LastTestCase { get; set; }
        public DateTime JudgedAt { get; set; }

        #endregion

        #region Submission Hacking
        
        public DateTime HackedAt { get; set; }
        public string HackerId { get; set; }
        [ForeignKey("HackerId")] public ApplicationUser Hacker { get; set; }

        #endregion
    }

    public class Hack : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Relationships

        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public int ProblemId { get; set; }
        public Problem Problem { get; set; }
        
        public int? AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        
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
        
        public int? ProblemId { get; set; }
        public Problem Problem { get; set; }
        
        public int? AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        
        public int? SubmissionId { get; set; }
        public Submission Submission { get; set; }
        
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
}