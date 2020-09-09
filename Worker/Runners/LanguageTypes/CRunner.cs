using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Worker.Models;

namespace Worker.Runners.LanguageTypes
{
    public class CRunner : LanguageRunnerBase
    {
        public CRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            Logger = provider.GetRequiredService<ILogger<CRunner>>();
        }

        protected override async Task<JudgeResult> CompileAsync()
        {
            var file = Path.Combine(Box, "program.c");
            var program = Convert.FromBase64String(Submission.Program.Code);
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(program);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = "--cg -s -E PATH=/usr/bin/ -i /dev/null -r compiler_output" +
                                " -p120 -f 409600 --cg-timing -t 15.0 -x 0 -w 20.0 -k 128000 --cg-mem=512000" +
                                " --run -- /usr/bin/gcc " +
                                LanguageOptions.LanguageOptionsDict[Language.C].CompilerOptions +
                                " program.c -o program"
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                return null;
            }
            else if (process.ExitCode != 1)
            {
                throw new Exception($"Isolate error ExitCode={process.ExitCode}.");
            }

            Logger.LogInformation($"Compilation ERROR gcc exited with non-zero code.");
            var compilerOutputFile = Path.Combine(Box, "compiler_output");
            await using var compilerOutputStream = new FileStream(compilerOutputFile, FileMode.Open);
            using var compilerOutputReader = new StreamReader(compilerOutputStream);
            var compilerOutputString = await compilerOutputReader.ReadToEndAsync();

            return new JudgeResult
            {
                Verdict = Verdict.CompilationError,
                Time = null,
                Memory = null,
                FailedOn = 0,
                Score = 0,
                Message = compilerOutputString
            };
        }

        protected override Task ExecuteProgramAsync(string meta, int bytes)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = $"--cg -s -M {meta} -i input -o output -r stderr -p1 -f {bytes}" +
                                $" --cg-timing -t {TimeLimit} -x 0 -w {TimeLimit + 3.0f} -k 128000 --cg-mem={MemoryLimit}" +
                                " --run -- program"
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0 && process.ExitCode != 1)
            {
                throw new Exception($"Isolate error ExitCode={process.ExitCode}.");
            }

            return Task.CompletedTask;
        }
    }
}