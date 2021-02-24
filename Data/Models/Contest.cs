using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Generics;

namespace Data.Models
{
    public enum ContestMode
    {
        Practice = 0, // Practice or exam
        OneShot = 1, // OI (judge only once)
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

        [DefaultValue(false)] public bool HasScoreBonus { get; set; }
        public DateTime? ScoreBonusTime { get; set; }
        public int? ScoreBonusPercentage { get; set; }

        [DefaultValue(false)] public bool HasScoreDecay { get; set; }
        public bool? IsScoreDecayLinear { get; set; }
        public DateTime? ScoreDecayTime { get; set; }
        public int? ScoreDecayPercentage { get; set; }

        #endregion

        #region Relationships

        public List<Problem> Problems { get; set; }
        public List<Registration> Registrations { get; set; }

        #endregion
    }
}