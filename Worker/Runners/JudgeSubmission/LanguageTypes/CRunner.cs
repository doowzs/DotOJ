using System;
using System.IO;
using System.Threading.Tasks;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes
{
    public class CRunner : Base.Runner
    {
        public CRunner(Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            Logger = provider.GetRequiredService<ILogger<CRunner>>();
        }

        protected override async Task<bool> CompileAsync()
        {
            var file = Path.Combine(Jail, "main.c");
            var program = Convert.FromBase64String(Submission.Program.Code);
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(program);
            }

            var options = LanguageOptions.LanguageOptionsDict[Language.C].CompilerOptions;
            return await Box.ExecuteAsync(
                $"/usr/bin/gcc {options} -o main main.c",
                bind: new[] {"/etc"},
                chroot: "jail",
                stderr: "compiler_output",
                proc: 120,
                time: 15.0f,
                memory: 512000
            ) == 0;
        }

        protected override async Task ExecuteProgramAsync(string meta, int bytes)
        {
            await Box.ExecuteAsync(
                "main",
                meta: meta,
                chroot: "jail",
                stdin: "jail/input",
                stdout: "jail/output",
                stderr: "jail/stderr",
                proc: 1,
                disk: bytes,
                time: TimeLimit,
                memory: MemoryLimit
            );
        }
    }
}