using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Generics;
using Shared.Models;

namespace Shared.DTOs
{
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
        public bool HasScoreBonus { get; }
        public DateTime? ScoreBonusTime { get; }
        public int? ScoreBonusPercentage { get; }
        public bool HasScoreDecay { get; }
        public bool? IsScoreDecayLinear { get; }
        public DateTime? ScoreDecayTime { get; }
        public int? ScoreDecayPercentage { get; }
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
            HasScoreBonus = contest.HasScoreBonus;
            ScoreBonusTime = contest.ScoreBonusTime;
            ScoreBonusPercentage = contest.ScoreBonusPercentage;
            HasScoreDecay = contest.HasScoreDecay;
            IsScoreDecayLinear = contest.IsScoreDecayLinear;
            ScoreDecayTime = contest.ScoreDecayTime;
            ScoreDecayPercentage = contest.ScoreDecayPercentage;
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
        [Required] public bool? HasScoreBonus { get; set; }
        public DateTime? ScoreBonusTime { get; set; }
        public int? ScoreBonusPercentage { get; set; }
        [Required] public bool? HasScoreDecay { get; set; }
        public bool? IsScoreDecayLinear { get; set; }
        public DateTime? ScoreDecayTime { get; set; }
        public int? ScoreDecayPercentage { get; set; }

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
            HasScoreBonus = contest.HasScoreBonus;
            ScoreBonusTime = contest.ScoreBonusTime;
            ScoreBonusPercentage = contest.ScoreBonusPercentage;
            HasScoreDecay = contest.HasScoreDecay;
            IsScoreDecayLinear = contest.IsScoreDecayLinear;
            ScoreDecayTime = contest.ScoreDecayTime;
            ScoreDecayPercentage = contest.ScoreDecayPercentage;
        }
    }
}