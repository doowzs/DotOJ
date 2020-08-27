using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{
    [NotMapped]
    public class ProblemStatistics
    {
        public int ProblemId { get; set; }
        public int Penalties { get; set; }
        public DateTime AcceptedAt { get; set; }
        public int Score { get; set; }
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

        [NotMapped] public List<ProblemStatistics> Statistics;

        [Column("statistics", TypeName = "text")]
        public string StatisticsSerialized
        {
            get => JsonConvert.SerializeObject(Statistics);
            set => Statistics = string.IsNullOrEmpty(value)
                ? new List<ProblemStatistics>()
                : JsonConvert.DeserializeObject<List<ProblemStatistics>>(value);
        }

        #endregion
    }
    
    [NotMapped]
    public class RegistrationInfoDto : DtoWithTimestamps
    {
        public string UserId { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ContestId { get; }
        public bool IsParticipant { get; }
        public bool IsContestManager { get; }
        public List<ProblemStatistics> Statistics { get; }
        
        public RegistrationInfoDto(Registration registration) : base(registration)
        {
            UserId = registration.UserId;
            ContestId = registration.ContestId;
            ContestantId = registration.User.ContestantId;
            ContestantName = registration.User.ContestantName;
            IsParticipant = registration.IsParticipant;
            IsContestManager = registration.IsContestManager;
            Statistics = registration.Statistics;
        }
    }
}