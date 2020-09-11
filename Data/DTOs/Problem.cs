using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Generics;
using Data.Models;

namespace Data.DTOs
{
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
            ContestId = problem.ContestId;
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

    public class ProblemEditDto : DtoWithTimestamps
    {
        public int? Id { get; }
        [Required] public int? ContestId { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Description { get; set; }
        [Required] public string InputFormat { get; set; }
        [Required] public string OutputFormat { get; set; }
        public string FootNote { get; set; }

        [Required, Range(100, 30000)] public int? TimeLimit { get; set; }
        [Required, Range(1000, 512000)] public int? MemoryLimit { get; set; }

        [Required] public bool HasSpecialJudge { get; set; }
        public Program SpecialJudgeProgram { get; set; }

        [Required] public bool HasHacking { get; set; }
        public Program StandardProgram { get; set; }
        public Program ValidatorProgram { get; set; }

        [Required] public List<TestCase> SampleCases { get; set; }

        public ProblemEditDto()
        {
        }

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
            SpecialJudgeProgram = problem.SpecialJudgeProgram;
            HasHacking = problem.HasHacking;
            StandardProgram = problem.StandardProgram;
            ValidatorProgram = problem.ValidatorProgram;
            SampleCases = problem.SampleCases;
        }
    }
}