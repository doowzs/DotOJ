using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    public enum ContestMode
    {
        Practice = 0,  // Practice or exam
        OneShot = 1,   // OI (judge only once)
        UntilFail = 2, // ICPC (until first fail)
        SampleOnly = 3 // CF (judge samples only)
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
        public List<Clarification> Clarifications { get; set; }
        public List<Registration> Registrations { get; set; }

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
        public IList<ProblemInfoDto> Problems { get; }

        public ContestViewDto(Contest contest, IList<ProblemInfoDto> problems) : base(contest)
        {
            Id = contest.Id;
            Title = contest.Title;
            Description = contest.Description;
            IsPublic = contest.IsPublic;
            Mode = contest.Mode;
            BeginTime = contest.BeginTime;
            EndTime = contest.EndTime;
            Problems = problems;
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
}
