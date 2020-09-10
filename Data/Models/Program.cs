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
        /*
        CSharp = 51,
        Haskell = 61,
        Java13 = 62,
        JavaScript = 63,
        Lua = 64,
        Php = 68,
        Python3 = 71,
        Ruby = 72,
        Rust = 73,
        TypeScript = 74,
        */
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
        RuntimeError = 8,
        InternalError = 9
    }

    [NotMapped]
    public class Program
    {
        [Required] public Language? Language { get; set; }
        [Required, MaxLength(40960)] public string Code { get; set; }
    }
}