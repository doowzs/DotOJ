using System;
using System.IO;
using System.Threading.Tasks;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes
{
    public class CSharpRunner : Base.Runner
    {
        public CSharpRunner(Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            Logger = provider.GetRequiredService<ILogger<CSharpRunner>>();
        }

        protected override async Task<bool> CompileAsync()
        {
            var file = Path.Combine(Jail, "main.cs");
            var program = Convert.FromBase64String(Submission.Program.Code);
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(program);
            }

            var options = LanguageOptions.LanguageOptionsDict[Language.CSharp].CompilerOptions;
            return await Box.ExecuteAsync(
                "/usr/bin/csc " + options + " -out:main.exe main.cs",
                bind: new[] {"/etc"},
                chroot: "jail",
                stdout: "compiler_output", // Roslyn prints all output to stdout
                proc: 120,
                time: 15.0f,
                memory: 512000
            ) == 0;
        }

        protected override async Task ExecuteProgramAsync(string meta, int bytes)
        {
            await Box.ExecuteAsync(
                "/usr/bin/csr main.exe",
                meta: meta,
                chroot: "jail",
                stdin: "jail/input",
                stdout: "jail/output",
                stderr: "jail/stderr",
                proc: 20,
                disk: bytes,
                time: TimeLimit,
                memory: MemoryLimit
            );
        }
    }
}