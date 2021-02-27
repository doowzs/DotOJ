using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Generics;
using Shared.Models;

namespace Shared.DTOs
{
    public class ProblemStatistics
    {
        public int TotalSubmissions;
        public int AcceptedSubmissions;
        public int TotalContestants;
        public int AcceptedContestants;
        public Dictionary<Verdict, int> ByVerdict;
        public DateTime UpdatedAt;
    }

    public class ProblemInfoDto
    {
        public int Id { get; }
        public int ContestId { get; }
        public string Title { get; }
        public bool Attempted { get; }
        public bool Solved { get; }
        public int TotalContestants { get; }
        public int AcceptedContestants { get; }

        public ProblemInfoDto(Problem problem)
        {
            Id = problem.Id;
            ContestId = problem.ContestId;
            Title = problem.Title;
            Attempted = false;
            Solved = false;
            TotalContestants = 0;
            AcceptedContestants = 0;
        }

        public ProblemInfoDto(Problem problem, bool attempted, bool solved, 
            ProblemStatistics statistics) : this(problem)
        {
            Attempted = attempted;
            Solved = solved;
            TotalContestants = statistics.TotalContestants;
            AcceptedContestants = statistics.AcceptedContestants;
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
        public ProblemStatistics Statistics { get; }

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
            Statistics = null;
        }

        public ProblemViewDto(Problem problem, ProblemStatistics statistics) : this(problem)
        {
            Statistics = statistics;
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