using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    public enum Language
    {
        Text = 0,
        C = 1,
        Cpp = 2,
        Java = 3,
        Python = 4,
        Golang = 5,
        Rust = 6,
        CSharp = 7,
        Haskell = 8,
        LabArchive = 9
    }

    public enum Verdict
    {
        Rejected = -3,
        Voided = -2,
        Failed = -1,
        Pending = 0,
        // InQueue = 1, [deprecated]
        Running = 2,
        Accepted = 3,
        WrongAnswer = 4,
        TimeLimitExceeded = 5,
        MemoryLimitExceeded = 6,
        CompilationError = 7,
        RuntimeError = 8,
        CustomInputOk = 9,
    }

    [NotMapped]
    public class Program
    {
        [Required] public Language? Language { get; set; }
        [Required, MaxLength(54616) /* base64 of 40960 */] public string Code { get; set; }
        [MaxLength(5464) /* base64 of 4096 */] public string Input { get; set; }

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
                Models.Language.LabArchive => ".zip",
                _ => ".txt"
            };
        }

        public string GetSourceFileCommentSign()
        {
            return Language switch
            {
                Models.Language.Python => "# ",
                Models.Language.Haskell => "-- ",
                Models.Language.LabArchive => "",
                _ => "// "
            };
        }
    }
}