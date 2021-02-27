using System.Collections;
using System.Collections.Generic;
using Shared.Models;

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
            LanguageId = languageId;
            TimeFactor = timeFactor;
            CompilerOptions = compilerOptions;
        }

        public static readonly IDictionary<Language, LanguageOptions> LanguageOptionsDict =
            new Dictionary<Language, LanguageOptions>()
            {
                {
                    Language.C, new LanguageOptions(1, 1.0f,
                        "-std=c11 -static -march=native -O2 -fno-strict-aliasing -DONLINE_JUDGE")
                },
                {
                    Language.Cpp, new LanguageOptions(2, 1.0f,
                        "-std=c++17 -static -march=native -O2 -fno-strict-aliasing -DONLINE_JUDGE")
                },
                {Language.Java, new LanguageOptions(3, 2.0f, "-J-Xms64m -J-Xmx512m")},
                {Language.Python, new LanguageOptions(4, 5.0f)},
                {Language.Golang, new LanguageOptions(5, 2.0f)},
                {Language.Rust, new LanguageOptions(6, 2.5f, "-O")},
                {Language.CSharp, new LanguageOptions(7, 1.5f, "-optimize+ -define:ONLINE_JUDGE")},
                {Language.Haskell, new LanguageOptions(61, 2.5f, "-v0 -O")},
            };
    }
}