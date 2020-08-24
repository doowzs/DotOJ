using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{
    public enum ContestMode
    {
        Practice = 0,  // Practice or exam
        OneShot = 1,   // OI (judge only once)
        UntilFail = 2, // ICPC (until first fail)
        SampleOnly = 3 // CF (judge samples only)
    }

    [NotMapped]
    public class ContestProblemStatus
    {
        public int ProblemId { get; set; }
        public int Penalties { get; set; }
        public DateTime AcceptedAt { get; set; }
        public int Score { get; set; }
    }

    [NotMapped]
    public class ContestParticipantStatistics
    {
        public List<ContestProblemStatus> Statuses { get; set; }

        public int SuccessfulHacks { get; set; }
        public int TotalHacks { get; set; }
        
        public int TotalScore { get; set; }

        public ContestParticipantStatistics()
        {
            Statuses = new List<ContestProblemStatus>();
        }
    }

    public class Contest : ModelWithTimestamps
    {
        public int Id { get; set; }

        #region Contest Content

        [Required] public string Title { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }

        [Required] public bool IsPublic { get; set; }
        [Required] public ContestMode Mode { get; set; }

        [Required] public DateTime BeginTime { get; set; }
        [Required] public DateTime EndTime { get; set; }

        #endregion

        #region Relationships

        public List<Problem> Problems { get; set; }
        public List<ContestNotice> Notices { get; set; }
        public List<ContestRegistration> Registrations { get; set; }

        #endregion
    }

    public class ContestNotice : ModelWithTimestamps
    {
        public int Id { get; set; }

        public int ContestId { get; set; }
        public Contest Contest { get; set; }

        [Required, Column(TypeName = "text")] public string Content { get; set; }
    }

    public class ContestRegistration : ModelWithTimestamps
    {
        // Table uses composite key: (UserID, ContestId).
        [Required] public string UserId { get; set; }
        public int ContestId { get; set; }
        public ApplicationUser User { get; set; }
        public Contest Contest { get; set; }

        public bool IsParticipant { get; set; }
        public bool IsContestManager { get; set; }

        #region Statistics

        [NotMapped] public ContestParticipantStatistics Statistics;

        [Column("statistics", TypeName = "text")]
        public string StatisticsSerialized
        {
            get => JsonConvert.SerializeObject(Statistics);
            set => Statistics = string.IsNullOrEmpty(value)
                ? new ContestParticipantStatistics()
                : JsonConvert.DeserializeObject<ContestParticipantStatistics>(value);
        }

        #endregion
    }

    public class ContestInfoDto
    {
        public int Id { get; }
        public string Title { get; }
        public bool IsPublic { get; }
        public ContestMode Mode { get; }
        public DateTime BeginTime { get; }
        public DateTime EndTime { get; }
        public bool Registered { get; }

        public ContestInfoDto(Contest contest, bool registered)
        {
            Id = contest.Id;
            Title = contest.Title;
            IsPublic = contest.IsPublic;
            Mode = contest.Mode;
            BeginTime = contest.BeginTime;
            EndTime = contest.EndTime;
            Registered = registered;
        }
    }

    public class ContestViewDto : DtoWithTimestamps 
    {
        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public bool IsPublic { get; }
        public ContestMode Mode { get; }
        public DateTime BeginTime { get; }
        public DateTime EndTime { get; }
        public List<ProblemInfoDto> Problems { get; }
        public List<ContestNoticeDto> Notices { get; }

        public ContestViewDto(Contest contest) : base(contest)
        {
            Id = contest.Id;
            Title = contest.Title;
            Description = contest.Description;
            IsPublic = contest.IsPublic;
            Mode = contest.Mode;
            BeginTime = contest.BeginTime;
            EndTime = contest.EndTime;
            
            Problems = new List<ProblemInfoDto>();
            foreach (var problem in contest.Problems)
            {
                Problems.Add(new ProblemInfoDto(problem));
            }
            
            Notices = new List<ContestNoticeDto>();
            foreach (var notice in contest.Notices)
            {
                Notices.Add(new ContestNoticeDto(notice));
            }
        }
    }

    public class ContestEditDto : DtoWithTimestamps
    {
        public int? Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        [Required] public bool? IsPublic { get; set; }
        [Required] public ContestMode? Mode { get; set; }
        [Required] public DateTime BeginTime { get; set; }
        [Required] public DateTime EndTime { get; set; }

        public ContestEditDto()
        {
        }
        
        public ContestEditDto(Contest contest) : base(contest)
        {
            Id = contest.Id;
            Title = contest.Title;
            Description = contest.Description;
            IsPublic = contest.IsPublic;
            Mode = contest.Mode;
            BeginTime = contest.BeginTime;
            EndTime = contest.EndTime;
        }
    }

    public class ContestNoticeDto : DtoWithTimestamps
    {
        public int Id { get; }
        [Required] public int? ContestId { get; }
        [Required] public string Content { get; }
        public ContestNoticeDto(ContestNotice notice) : base(notice)
        {
            Id = notice.Id;
            ContestId = notice.ContestId;
            Content = notice.Content;
        }
    }

    public class ContestRegistrationDto : DtoWithTimestamps
    {
        [Required] public string UserId { get; }
        public string UserName { get; }
        [Required] public int? ContestId { get; }
        [Required] public bool? IsParticipant { get; }
        [Required] public bool? IsContestManager { get; }
        public ContestParticipantStatistics Statistics { get; }
        
        public ContestRegistrationDto(ContestRegistration registration) : base(registration)
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
