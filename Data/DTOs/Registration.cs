using System.Collections.Generic;
using Data.Generics;
using Data.Models;

namespace Data.DTOs
{
    public class RegistrationInfoDto : DtoWithTimestamps
    {
        public string UserId { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ContestId { get; }
        public bool IsParticipant { get; }
        public bool IsContestManager { get; }
        public List<RegistrationProblemStatistics> Statistics { get; }

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