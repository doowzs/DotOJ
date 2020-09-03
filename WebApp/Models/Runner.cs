using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace WebApp.Models
{
    [NotMapped]
    public class RunnerLanguageOptions
    {
        public int languageId { get; }
        public float timeFactor { get; }
        public string compilerOptions { get; }

        public RunnerLanguageOptions(int languageId, float timeFactor) : this(languageId, timeFactor, "")
        {
        }

        public RunnerLanguageOptions(int languageId, float timeFactor, string compilerOptions)
        {
            this.languageId = languageId;
            this.timeFactor = timeFactor;
            this.compilerOptions = compilerOptions;
        }

        public static IDictionary<Language, RunnerLanguageOptions> LanguageOptionsDict =
            new Dictionary<Language, RunnerLanguageOptions>()
            {
                {Language.C, new RunnerLanguageOptions(50, 1.0f, "-DONLINE_JUDGE --static -O2 --std=c11")},
                {Language.CSharp, new RunnerLanguageOptions(51, 1.5f)},
                {Language.Cpp, new RunnerLanguageOptions(54, 1.0f, "-DONLINE_JUDGE --static -O2 --std=c++17")},
                {Language.Go, new RunnerLanguageOptions(60, 2.0f)},
                {Language.Haskell, new RunnerLanguageOptions(61, 2.5f)},
                {Language.Java13, new RunnerLanguageOptions(62, 2.0f, "-J-Xms32m -J-Xmx256m")},
                {Language.JavaScript, new RunnerLanguageOptions(63, 5.0f)},
                {Language.Lua, new RunnerLanguageOptions(64, 6.0f)},
                {Language.Php, new RunnerLanguageOptions(68, 4.5f)},
                {Language.Python3, new RunnerLanguageOptions(71, 5.0f)},
                {Language.Ruby, new RunnerLanguageOptions(72, 5.0f)},
                {Language.Rust, new RunnerLanguageOptions(73, 2.5f)},
                {Language.TypeScript, new RunnerLanguageOptions(74, 5.0f)}
            };
    }

    [NotMapped]
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
            LanguageId = RunnerLanguageOptions.LanguageOptionsDict[language].languageId;
            CompilerOptions = RunnerLanguageOptions.LanguageOptionsDict[language].compilerOptions;
            Stdin = input;
            ExpectedOutput = output;
            CpuTimeLimit = submission.Problem.TimeLimit *
                RunnerLanguageOptions.LanguageOptionsDict[language].timeFactor / 1000;
            MemoryLimit = submission.Problem.MemoryLimit;
        }
    }

    [NotMapped]
    public class RunnerToken
    {
        [JsonProperty("token")] public string Token { get; set; }
    }

    [NotMapped]
    public class RunnerStatus
    {
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("compile_output")] public string CompileOutput { get; set; }
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("wall_time")] public string WallTime { get; set; }
        [JsonProperty("memory")] public float? Memory { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("status_id")] public Verdict Verdict { get; set; }
    }

    [NotMapped]
    public class RunnerResponse
    {
        [JsonProperty("submissions")] public List<RunnerStatus> Statuses { get; set; }
    }
}