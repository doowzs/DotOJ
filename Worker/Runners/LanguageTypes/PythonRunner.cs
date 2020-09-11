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
    public class PythonRunner : Base.Runner
    {
        public PythonRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            Logger = provider.GetRequiredService<ILogger<PythonRunner>>();
        }

        protected override async Task<JudgeResult> CompileAsync()
        {
            var file = Path.Combine(Jail, "main.py");
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
                    Arguments =
                        $"--cg -b {BoxId} -s -M {meta} -c jail -d /box={Box}:norec -d /box/jail={Jail}:rw" +
                        $" -i jail/input -o jail/output -r jail/stderr -p1 -f {bytes}" +
                        $" --cg-timing -t {TimeLimit} -x 0 -w {TimeLimit + 3.0f} -k 128000 --cg-mem={MemoryLimit}" +
                        " --run -- /usr/bin/python3 main.py"
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