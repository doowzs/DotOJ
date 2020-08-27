using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{

    [NotMapped]
    public class TestCase
    {
        // When used as sample cases, Input and Output are contents.
        // When used as test cases, Input and Output are path to files.
        public string Input { get; set; }
        public string Output { get; set; }
    }

    public class Problem : ModelWithTimestamps
    {
        public int Id { get; set; }
        
        public int ContestId { get; set; }
        public Contest Contest { get; set; }
        public List<Submission> Submissions { get; set; }
        
        #region Problem Description

        [Required] public string Title { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }
        [Required, Column(TypeName = "text")] public string InputFormat { get; set; }
        [Required, Column(TypeName = "text")] public string OutputFormat { get; set; }
        [Column(TypeName = "text")] public string FootNote { get; set; }

        #endregion

        #region Judgement Protocol

        [Range(0.0, Double.PositiveInfinity)] public double TimeLimit { get; set; }
        [Range(0.0, Double.PositiveInfinity)] public double MemoryLimit { get; set; }

        public bool HasSpecialJudge { get; set; }
        [NotMapped] public Program SpecialJudgeProgram { get; set; }

        [Column("SpecialJudgeProgram", TypeName = "text")]
        public string SpecialJudgeProgramSerialized
        {
            get => JsonConvert.SerializeObject(SpecialJudgeProgram);
            set => SpecialJudgeProgram = string.IsNullOrEmpty(value)
                ? null
                : JsonConvert.DeserializeObject<Program>(value);
        }

        public bool HasHacking { get; set; }
        [NotMapped] public Program StandardProgram { get; set; }

        [Column("StandardProgram", TypeName = "text")]
        public string StandardProgramSerialized
        {
            get => JsonConvert.SerializeObject(StandardProgram);
            set => StandardProgram = string.IsNullOrEmpty(value)
                ? null
                : JsonConvert.DeserializeObject<Program>(value);
        }

        [NotMapped] public Program ValidatorProgram { get; set; }

        [Column("ValidatorProgram", TypeName = "text")]
        public string ValidatorProgramSerialized
        {
            get => JsonConvert.SerializeObject(ValidatorProgram);
            set => ValidatorProgram = string.IsNullOrEmpty(value)
                ? null
                : JsonConvert.DeserializeObject<Program>(value);
        }

        [NotMapped] public List<TestCase> SampleCases;
        [NotMapped] public List<TestCase> TestCases;

        [Required, Column("SampleCases", TypeName = "text")]
        public string SampleCasesSerialized
        {
            get => JsonConvert.SerializeObject(SampleCases);
            set =>
                SampleCases = string.IsNullOrEmpty(value)
                    ? new List<TestCase>()
                    : JsonConvert.DeserializeObject<List<TestCase>>(value);
        }

        [Required, Column("TestCases", TypeName = "text")]
        public string TestCasesSerialized
        {
            get => JsonConvert.SerializeObject(TestCases);
            set =>
                TestCases = string.IsNullOrEmpty(value)
                    ? new List<TestCase>()
                    : JsonConvert.DeserializeObject<List<TestCase>>(value);
        }

        #endregion
    }

    #region Data Transfer Objects

    [NotMapped]
    public class ProblemInfoDto
    {
        public int Id { get; }
        public int ContestId { get; }
        public string Title { get; }
        public bool Solved { get; }
        
        public ProblemInfoDto(Problem problem)
        {
            Id = problem.Id;
            ContestId = problem.ContestId;
            Title = problem.Title;
            Solved = false;
        }

        public ProblemInfoDto(Problem problem, bool solved) : this(problem)
        {
            Solved = solved;
        }
    }
    
    [NotMapped]
    public class ProblemViewDto : DtoWithTimestamps
    {
        public int Id { get; }
        public int ContestId { get; }
        public string Title { get; }
        public string Description { get; }
        public string InputFormat { get; }
        public string OutputFormat { get; }
        public string FootNote { get; }

        public double TimeLimit { get; }
        public double MemoryLimit { get; }

        public bool HasSpecialJudge { get; }
        public bool HasHacking { get; }
        public List<TestCase> SampleCases { get; }
        
        public ProblemViewDto(Problem problem) : base(problem)
        {
            Id = problem.Id;
            ContestId = problem.Id;
            Title = problem.Title;
            Description = problem.Description;
            InputFormat = problem.InputFormat;
            OutputFormat = problem.OutputFormat;
            FootNote = problem.FootNote;
            TimeLimit = problem.TimeLimit;
            MemoryLimit = problem.MemoryLimit;
            HasSpecialJudge = problem.HasSpecialJudge;
            HasHacking = problem.HasHacking;
            SampleCases = problem.SampleCases;
        }
    }

    [NotMapped]
    public class ProblemEditDto : DtoWithTimestamps
    {
        public int Id { get; }
        [Required] public int? ContestId { get; }
        [Required] public string Title { get; }
        [Required] public string Description { get; }
        [Required] public string InputFormat { get; }
        [Required] public string OutputFormat { get; }
        [Required] public string FootNote { get; }

        [Required, Range(0.0, Double.PositiveInfinity)] public double? TimeLimit { get; }
        [Required, Range(0.0, Double.PositiveInfinity)] public double? MemoryLimit { get; }

        [Required] public bool HasSpecialJudge { get; }
        public string SpecialJudgeProgram { get; }

        [Required] public bool HasHacking { get; }
        public string StandardProgram { get; }
        public string ValidatorProgram { get; }

        [Required] public List<TestCase> SampleCases { get; }
        [Required] public List<TestCase> TestCases { get; }

        public ProblemEditDto(Problem problem) : base(problem)
        {
            Id = problem.Id;
            ContestId = problem.ContestId;
            Title = problem.Title;
            Description = problem.Description;
            InputFormat = problem.InputFormat;
            OutputFormat = problem.OutputFormat;
            FootNote = problem.FootNote;
            TimeLimit = problem.TimeLimit;
            MemoryLimit = problem.MemoryLimit;
            HasSpecialJudge = problem.HasSpecialJudge;
            SpecialJudgeProgram = problem.SpecialJudgeProgramSerialized;
            HasHacking = problem.HasHacking;
            StandardProgram = problem.StandardProgramSerialized;
            ValidatorProgram = problem.ValidatorProgramSerialized;
            SampleCases = problem.SampleCases;
            TestCases = problem.TestCases;
        }
    }

    #endregion
}
