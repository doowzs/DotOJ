using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{
    public class ProblemStatus
    {
        public int ProblemId { get; set; }
        public int Penalties { get; set; }
        public DateTime AcceptedAt { get; set; }
        public int Score { get; set; }
    }

    [NotMapped]
    public class ParticipantStatistics
    {
        public List<ProblemStatus> Statuses { get; set; }

        public int SuccessfulHacks { get; set; }
        public int TotalHacks { get; set; }
        
        public int TotalScore { get; set; }

        public ParticipantStatistics()
        {
            Statuses = new List<ProblemStatus>();
        }
    }
    
    public class Registration : ModelWithTimestamps
    {
        // Table uses composite key: (UserID, ContestId).
        [Required] public string UserId { get; set; }
        public int ContestId { get; set; }
        public ApplicationUser User { get; set; }
        public Contest Contest { get; set; }

        public bool IsParticipant { get; set; }
        public bool IsContestManager { get; set; }

        #region Statistics

        [NotMapped] public ParticipantStatistics Statistics;

        [Column("statistics", TypeName = "text")]
        public string StatisticsSerialized
        {
            get => JsonConvert.SerializeObject(Statistics);
            set => Statistics = string.IsNullOrEmpty(value)
                ? new ParticipantStatistics()
                : JsonConvert.DeserializeObject<ParticipantStatistics>(value);
        }

        #endregion
    }
    
    public class RegistrationDto : DtoWithTimestamps
    {
        [Required] public string UserId { get; }
        public string UserName { get; }
        [Required] public int? ContestId { get; }
        [Required] public bool? IsParticipant { get; }
        [Required] public bool? IsContestManager { get; }
        public ParticipantStatistics Statistics { get; }
        
        public RegistrationDto(Registration registration) : base(registration)
        {
            UserId = registration.UserId;
            UserName = registration.User.UserName;
            ContestId = registration.ContestId;
            IsParticipant = registration.IsParticipant;
            IsContestManager = registration.IsContestManager;
            Statistics = registration.Statistics;
        }
    }
}