using System.Collections;
using System.Collections.Generic;
using Data.Models;

namespace Worker.Models
{
    public class LanguageOptions
    {
        public int LanguageId { get; }
        public float TimeFactor { get; }
        public string CompilerOptions { get; }

        public LanguageOptions(int languageId, float timeFactor) : this(languageId, timeFactor, "")
        {
        }

        public LanguageOptions(int languageId, float timeFactor, string compilerOptions)
        {
            this.LanguageId = languageId;
            this.TimeFactor = timeFactor;
            this.CompilerOptions = compilerOptions;
        }

        public static readonly IDictionary<Language, LanguageOptions> LanguageOptionsDict =
            new Dictionary<Language, LanguageOptions>()
            {
                {Language.C, new LanguageOptions(50, 1.0f, "-DONLINE_JUDGE --static -O2 --std=c11")},
                {Language.CSharp, new LanguageOptions(51, 1.5f)},
                {Language.Cpp, new LanguageOptions(54, 1.0f, "-DONLINE_JUDGE --static -O2 --std=c++17")},
                {Language.Go, new LanguageOptions(60, 2.0f)},
                {Language.Haskell, new LanguageOptions(61, 2.5f)},
                {Language.Java13, new LanguageOptions(62, 2.0f, "-J-Xms32m -J-Xmx256m")},
                {Language.JavaScript, new LanguageOptions(63, 5.0f)},
                {Language.Lua, new LanguageOptions(64, 6.0f)},
                {Language.Php, new LanguageOptions(68, 4.5f)},
                {Language.Python3, new LanguageOptions(71, 5.0f)},
                {Language.Ruby, new LanguageOptions(72, 5.0f)},
                {Language.Rust, new LanguageOptions(73, 2.5f)},
                {Language.TypeScript, new LanguageOptions(74, 5.0f)}
            };
    }
}