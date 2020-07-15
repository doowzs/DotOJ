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

        #region Problem Designer

        [Required] public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        #endregion

        #region Problem Description

        [Required] public string Name { get; set; }
        [Required, Column(TypeName = "text")] public string Description { get; set; }
        [Required, Column(TypeName = "text")] public string InputFormat { get; set; }
        [Required, Column(TypeName = "text")] public string OutputFormat { get; set; }
        [Column(TypeName = "text")] public string FootNote { get; set; }

        #endregion

        #region Judgement Protocol

        public double TimeLimit { get; set; }
        public double MemoryLimit { get; set; }

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

        #region Submission Statistics

        public int AcceptedSubmissions;
        public int TotalSubmissions;

        #endregion
    }

    #region Data Transfer Objects

    [NotMapped]
    public class ProblemInfoDto
    {
        public int Id { get; }
        public string Name { get; }
        public int AcceptedSubmissions { get; }
        public int TotalSubmissions { get; }

        public ProblemInfoDto(Problem problem)
        {
            Id = problem.Id;
            Name = problem.Name;
            AcceptedSubmissions = problem.AcceptedSubmissions;
            TotalSubmissions = problem.TotalSubmissions;
        }
    }
    
    [NotMapped]
    public class ProblemViewDto : DtoWithTimestamps
    {
        public int Id { get; }
        public string UserId { get; }
        public string UserName { get; }

        public string Name { get; }
        public string Description { get; }
        public string InputFormat { get; }
        public string OutputFormat { get; }
        public string FootNote { get; }

        public double TimeLimit { get; }
        public double MemoryLimit { get; }

        public bool HasSpecialJudge { get; }
        public bool HasHacking { get; }
        public string SampleCases { get; }
        
        public int AcceptedSubmissions { get; set; }
        public int TotalSubmissions { get; set; }

        public ProblemViewDto(Problem problem) : base(problem)
        {
            Id = problem.Id;
            UserId = problem.UserId;
            UserName = problem.User?.UserName;
            Name = problem.Name;
            Description = problem.Description;
            InputFormat = problem.InputFormat;
            OutputFormat = problem.OutputFormat;
            FootNote = problem.FootNote;
            TimeLimit = problem.TimeLimit;
            MemoryLimit = problem.MemoryLimit;
            HasSpecialJudge = problem.HasSpecialJudge;
            HasHacking = problem.HasHacking;
            SampleCases = problem.SampleCasesSerialized;
            AcceptedSubmissions = problem.AcceptedSubmissions;
            TotalSubmissions = problem.TotalSubmissions;
        }
    }

    [NotMapped]
    public class ProblemEditDto : DtoWithTimestamps
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string InputFormat { get; set; }
        public string OutputFormat { get; set; }
        public string FootNote { get; set; }

        public double TimeLimit { get; set; }
        public double MemoryLimit { get; set; }

        public bool HasSpecialJudge { get; set; }
        public string SpecialJudgeProgram { get; set; }

        public bool HasHacking { get; set; }
        public string StandardProgram { get; set; }
        public string ValidatorProgram { get; set; }

        public string SampleCases { get; set; }
        public string TestCases { get; set; }

        public ProblemEditDto(Problem problem) : base(problem)
        {
            Id = problem.Id;
            UserId = problem.UserId;
            UserName = problem.User?.UserName;
            Name = problem.Name;
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
            SampleCases = problem.SampleCasesSerialized;
            TestCases = problem.TestCasesSerialized;
        }
    }

    #endregion
}
