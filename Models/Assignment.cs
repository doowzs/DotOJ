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

        [Required] public string Title { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }

        [Required] public bool IsPublic { get; set; }
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

    public class AssignmentInfoDto
    {
        public int Id { get; }
        public string Title { get; }
        public bool IsPublic { get; }
        public AssignmentMode Mode { get; }
        public DateTime BeginTime { get; }
        public DateTime EndTime { get; }
        public int Registered { get; }

        public AssignmentInfoDto(Assignment assignment)
        {
            Id = assignment.Id;
            Title = assignment.Title;
            IsPublic = assignment.IsPublic;
            Mode = assignment.Mode;
            BeginTime = assignment.BeginTime;
            EndTime = assignment.EndTime;
            Registered = assignment.Registrations.Count;
        }
    }

    public class AssignmentViewDto : DtoWithTimestamps 
    {
        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public bool IsPublic { get; }
        public AssignmentMode Mode { get; }
        public DateTime BeginTime { get; }
        public DateTime EndTime { get; }
        public List<ProblemViewDto> Problems { get; }
        public List<AssignmentNoticeDto> Notices { get; }
        public List<AssignmentRegistrationDto> Registrations { get; }

        public AssignmentViewDto(Assignment assignment) : base(assignment)
        {
            Id = assignment.Id;
            Title = assignment.Title;
            Description = assignment.Description;
            IsPublic = assignment.IsPublic;
            Mode = assignment.Mode;
            BeginTime = assignment.BeginTime;
            EndTime = assignment.EndTime;
            
            Problems = new List<ProblemViewDto>();
            foreach (var problem in assignment.Problems)
            {
                Problems.Add(new ProblemViewDto(problem));
            }
            
            Notices = new List<AssignmentNoticeDto>();
            foreach (var notice in assignment.Notices)
            {
                Notices.Add(new AssignmentNoticeDto(notice));
            }

            Registrations = new List<AssignmentRegistrationDto>();
            foreach (var registration in assignment.Registrations)
            {
                Registrations.Add(new AssignmentRegistrationDto(registration));
            }
        }
    }

    public class AssignmentEditDto : DtoWithTimestamps
    {
        public int Id { get; }
        [Required] public string Title { get; }
        [Required] public string Description { get; }
        [Required] public bool? IsPublic { get; }
        [Required] public AssignmentMode? Mode { get; }
        [Required] public DateTime BeginTime { get; }
        [Required] public DateTime EndTime { get; }
        public List<ProblemInfoDto> Problems { get; }
        public List<AssignmentNoticeDto> Notices { get; }
        public List<AssignmentRegistrationDto> Registrations { get; }
        public AssignmentEditDto(Assignment assignment) : base(assignment)
        {
            Id = assignment.Id;
            Title = assignment.Title;
            Description = assignment.Description;
            IsPublic = assignment.IsPublic;
            Mode = assignment.Mode;
            BeginTime = assignment.BeginTime;
            EndTime = assignment.EndTime;
            
            Problems = new List<ProblemInfoDto>();
            foreach (var problem in assignment.Problems)
            {
                Problems.Add(new ProblemInfoDto(problem));
            }
            
            Notices = new List<AssignmentNoticeDto>();
            foreach (var notice in assignment.Notices)
            {
                Notices.Add(new AssignmentNoticeDto(notice));
            }

            Registrations = new List<AssignmentRegistrationDto>();
            foreach (var registration in assignment.Registrations)
            {
                Registrations.Add(new AssignmentRegistrationDto(registration));
            }
        }
    }

    public class AssignmentNoticeDto : AssignmentNotice
    {
        public int Id { get; }
        [Required] public int? AssignmentId { get; }
        [Required] public string Content { get; }
        public AssignmentNoticeDto(AssignmentNotice notice)
        {
            Id = notice.Id;
            AssignmentId = notice.AssignmentId;
            Content = notice.Content;
        }
    }

    public class AssignmentRegistrationDto : AssignmentRegistration
    {
        public string UserId { get; }
        public string UserName { get; }
        public int AssignmentId { get; }
        public bool IsParticipant { get; }
        public bool IsAssignmentManager { get; }
        public AssignmentParticipantStatistics Statistics { get; }
        
        public AssignmentRegistrationDto(AssignmentRegistration registration)
        {
            UserId = registration.UserId;
            UserName = registration.User.UserName;
            AssignmentId = registration.AssignmentId;
            IsParticipant = registration.IsParticipant;
            IsAssignmentManager = registration.IsAssignmentManager;
            Statistics = registration.Statistics;
        }
    }
}
