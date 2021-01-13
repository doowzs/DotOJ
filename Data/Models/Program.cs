﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    public enum Language
    {
        C = 1,
        Cpp = 2,
        Java = 3,
        Python = 4,
        Golang = 5,
        Rust = 6,
        CSharp = 7,
        Haskell = 8
    }

    public enum Verdict
    {
        Voided = -2,
        Failed = -1,
        Pending = 0,
        InQueue = 1,
        Running = 2,
        Accepted = 3,
        WrongAnswer = 4,
        TimeLimitExceeded = 5,
        MemoryLimitExceeded = 6,
        CompilationError = 7,
        RuntimeError = 8
    }

    [NotMapped]
    public class Program
    {
        [Required] public Language? Language { get; set; }
        [Required, MaxLength(40960)] public string Code { get; set; }

        public string GetSourceFileExtension()
        {
            return Language switch
            {
                Models.Language.C => ".c",
                Models.Language.Cpp => ".cpp",
                Models.Language.Java => ".java",
                Models.Language.Python => ".py",
                Models.Language.Golang => ".go",
                Models.Language.Rust => ".rs",
                Models.Language.CSharp => ".cs",
                Models.Language.Haskell => ".hs",
                _ => ".txt"
            };
        }

        public string GetSourceFileCommentSign()
        {
            return Language switch
            {
                Models.Language.Python => "# ",
                Models.Language.Haskell => "-- ",
                _ => "// "
            };
        }
    }
}