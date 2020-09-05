using System;
using Data.Models;
using Newtonsoft.Json;

namespace Worker.Models
{
    public class RunnerOptions
    {
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

        public RunnerOptions(Submission submission, string input, string output)
        {
            if (submission.Program is null)
            {
                throw new NullReferenceException("Problem of submission is not loaded.");
            }

            var language = submission.Program.Language.GetValueOrDefault();
            SourceCode = submission.Program.Code;
            LanguageId = LanguageOptions.LanguageOptionsDict[language].LanguageId;
            CompilerOptions = LanguageOptions.LanguageOptionsDict[language].CompilerOptions;
            Stdin = input;
            ExpectedOutput = output;
            CpuTimeLimit = submission.Problem.TimeLimit *
                LanguageOptions.LanguageOptionsDict[language].TimeFactor / 1000;
            MemoryLimit = submission.Problem.MemoryLimit;
        }
    }
}