using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{
    public enum AssignmentMode
    {
        OiMode = 1,  // No judge
        CpcMode = 2, // Judge immediately
        CfMode = 3,  // Judge samples only
    }

    [NotMapped]
    public class AssignmentProblemStatus
    {
        public int ProblemId { get; set; }
        public int Penalties { get; set; }
        public DateTime AcceptedAt { get; set; }
        public int Score { get; set; }
    }

    [NotMapped]
    public class AssignmentParticipantStatistics
    {
        public List<AssignmentProblemStatus> Statuses { get; set; }

        public int SuccessfulHacks { get; set; }
        public int TotalHacks { get; set; }
        
        public int TotalScore { get; set; }

        public AssignmentParticipantStatistics()
        {
            Statuses = new List<AssignmentProblemStatus>();
        }
    }

    public class Assignment : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Assignment Content

        [Required] public string Name { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }

        [Required] public AssignmentMode Mode { get; set; }

        [Required] public DateTime BeginTime { get; set; }
        [Required] public DateTime EndTime { get; set; }

        #endregion

        #region Relationships

        public List<Problem> Problems { get; set; }
        public List<AssignmentNotice> Notices { get; set; }
        public List<AssignmentRegistration> Registrations { get; set; }

        #endregion
    }

    public class AssignmentNotice : ModelWithTimestamps
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [Required, Column(TypeName = "text")] public string Content { get; set; }
    }

    public class AssignmentRegistration : ModelWithTimestamps
    {
        // Table uses composite key: (UserID, AssignmentId).
        [Required] public string UserId { get; set; }
        public int AssignmentId { get; set; }
        public ApplicationUser User { get; set; }
        public Assignment Assignment { get; set; }

        public bool IsParticipant { get; set; }
        public bool IsAssignmentManager { get; set; }

        #region Statistics

        [NotMapped] public AssignmentParticipantStatistics Statistics;

        [Column("statistics", TypeName = "text")]
        public string StatisticsSerialized
        {
            get => JsonConvert.SerializeObject(Statistics);
            set => Statistics = string.IsNullOrEmpty(value)
                ? new AssignmentParticipantStatistics()
                : JsonConvert.DeserializeObject<AssignmentParticipantStatistics>(value);
        }

        #endregion
    }
}