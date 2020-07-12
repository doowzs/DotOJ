using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Judge1.Models
{
    public enum Language
    {
        C99 = 50,
        C11 = 150,
        C14 = 250,
        CSharp = 51,
        Cpp03 = 54,
        Cpp11 = 154,
        Cpp14 = 254,
        Cpp17 = 354,
        Go = 60,
        Haskell = 61,
        Java8 = 27,
        Java13 = 62,
        JavaScript = 63,
        Lua = 64,
        Php = 68,
        Python3 = 71,
        Ruby = 72,
        Rust = 73,
        TypeScript = 74,
    }

    [NotMapped]
    public class TestCase
    {
        // When used as sample cases, Input and Output are contents.
        // When used as test cases, Input and Output are path to files.
        public string Input { get; set; }
        public string Output { get; set; }
    }

    [NotMapped]
    public class Program
    {
        public Language Language { get; set; }
        public string Code { get; set; }
    }

    public class Problem
    {
        public int Id { get; set; }

        #region Problem Designer

        public int UserId { get; set; }
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

        [Required, Column("ValidatorProgram", TypeName = "text")]
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

        [Column("TestCases", TypeName = "text")]
        public string TestCasesSerialized
        {
            get => JsonConvert.SerializeObject(TestCases);
            set =>
                TestCases = string.IsNullOrEmpty(value)
                    ? new List<TestCase>()
                    : JsonConvert.DeserializeObject<List<TestCase>>(value);
        }

        #endregion

        #region Timestamps

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt;

        #endregion
    }
}