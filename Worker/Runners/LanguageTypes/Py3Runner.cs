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
    public class Py3Runner : LanguageRunnerBase
    {
        public Py3Runner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            Logger = provider.GetRequiredService<ILogger<Py3Runner>>();
        }

        protected override async Task<JudgeResult> CompileAsync()
        {
            var file = Path.Combine(Box, "program.py");
            var program = Convert.FromBase64String(Submission.Program.Code);
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(program);
            }

            return null;
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
                                " --run -- /usr/bin/python3 program.py"
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