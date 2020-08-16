using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;

namespace Judge1.Models
{
    [NotMapped]
    public class RunnerLanguageOptions
    {
        public int languageId { get; }
        public string compilerOptions { get; }

        public RunnerLanguageOptions(int languageId) : this(languageId, "")
        {
        }

        public RunnerLanguageOptions(int languageId, string compilerOptions)
        {
            this.languageId = languageId;
            this.compilerOptions = compilerOptions;
        }

        public static IDictionary<Language, RunnerLanguageOptions> CompilerOptionsDict =
            new Dictionary<Language, RunnerLanguageOptions>()
            {
                {Language.C, new RunnerLanguageOptions(50, "-DONLINE_JUDGE --static -O2 --std=c11")},
                {Language.CSharp, new RunnerLanguageOptions(51)},
                {Language.Cpp, new RunnerLanguageOptions(54, "-DONLINE_JUDGE --static -O2 --std=c++17")},
                {Language.Go, new RunnerLanguageOptions(60)},
                {Language.Haskell, new RunnerLanguageOptions(61)},
                {Language.Java13, new RunnerLanguageOptions(62, "-J-Xms32m -J-Xmx256m")},
                {Language.JavaScript, new RunnerLanguageOptions(63)},
                {Language.Lua, new RunnerLanguageOptions(64)},
                {Language.Php, new RunnerLanguageOptions(68)},
                {Language.Python3, new RunnerLanguageOptions(71)},
                {Language.Ruby, new RunnerLanguageOptions(72)},
                {Language.Rust, new RunnerLanguageOptions(73)},
                {Language.TypeScript, new RunnerLanguageOptions(74)}
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
        [JsonProperty("memory_limit")] public float MemoryLimit { get; set; }

        public RunnerOptions(Submission submission, string input, string output)
        {
            if (submission.Program is null)
            {
                throw new NullReferenceException("Problem of submission is not loaded.");
            }
            
            SourceCode = submission.Program.Code;
            SourceCode = Convert.ToBase64String(Encoding.UTF8.GetBytes(submission.Program.Code));
            LanguageId = RunnerLanguageOptions
                .CompilerOptionsDict[submission.Program.Language.GetValueOrDefault()].languageId;
            CompilerOptions = RunnerLanguageOptions
                .CompilerOptionsDict[submission.Program.Language.GetValueOrDefault()].compilerOptions;
            Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            ExpectedOutput = Convert.ToBase64String(Encoding.UTF8.GetBytes(output));
            CpuTimeLimit = (float) submission.Problem.TimeLimit / 1000;
            MemoryLimit = (float) submission.Problem.MemoryLimit;
        }
    }

    [NotMapped]
    public class RunnerResponse
    {
        public class RunnerResponseStatus
        {
            [JsonProperty("id")] public Verdict Id { get; set; }
            [JsonProperty("description")] public string Description { get; set; }
        }

        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("compile_output")] public string CompileOutput { get; set; }
        [JsonProperty("stdout")] public string Stdout { get; set; }
        [JsonProperty("stderr")] public string Stderr { get; set; }
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("memory")] public float? Memory { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("status")] public RunnerResponseStatus Status { get; set; }
    }
}