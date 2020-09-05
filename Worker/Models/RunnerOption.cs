using System;
using Data.Models;
using Newtonsoft.Json;

namespace Worker.Models
{
    public class RunnerOptions
    {
        [JsonIgnore] public Language Language { get; set; }
        [JsonProperty("source_code")] public string SourceCode { get; set; }
        [JsonProperty("language_id")] public int LanguageId { get; set; }
        [JsonProperty("compiler_options")] public string CompilerOptions { get; set; }
        [JsonProperty("stdin")] public string Stdin { get; set; }
        [JsonProperty("expected_output")] public string ExpectedOutput { get; set; }
        [JsonProperty("cpu_time_limit")] public float CpuTimeLimit { get; set; }
        [JsonProperty("cpu_extra_time")] public float CpuExtraTime => 1.0f;
        [JsonProperty("wall_time_limit")] public float WallTimeLimit => CpuTimeLimit * 2.0f;
        [JsonProperty("memory_limit")] public float MemoryLimit { get; set; }
        [JsonProperty("stack_limit")] public float StackLimit => 64 * 1024.0f;
        [JsonProperty("additional_files")] public string AdditionalFiles { get; set; }

        public RunnerOptions(Problem problem, Submission submission)
        {
            Language = submission.Program.Language.GetValueOrDefault();
            CpuTimeLimit = problem.TimeLimit * LanguageOptions.LanguageOptionsDict[Language].TimeFactor / 1000;
            MemoryLimit = problem.MemoryLimit;
        }

        public RunnerOptions(Problem problem, Submission submission, string input, string output)
            : this(problem, submission)
        {
            SourceCode = submission.Program.Code;
            LanguageId = LanguageOptions.LanguageOptionsDict[Language].LanguageId;
            CompilerOptions = LanguageOptions.LanguageOptionsDict[Language].CompilerOptions;
            Stdin = input;
            ExpectedOutput = problem.HasSpecialJudge ? null : output;
        }

        public RunnerOptions(Problem problem, Submission submission, string additionalFiles)
            : this(problem, submission)
        {
            LanguageId = 89; // Magic number for multi-file programs.
            AdditionalFiles = additionalFiles;
        }
    }
}