using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker.Models
{
    public sealed class Box : IDisposable, IAsyncDisposable
    {
        private static string Id { get; set; }
        public static string Root { get; private set; }

        private Box()
        {
        }

        public static async Task InitBoxAsync()
        {
            var builder = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "hostname",
                    Arguments = "-i",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.OutputDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.ErrorDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception($"E: Cannot initialize isolate. Hostname exited with code {process.ExitCode}.\n" + builder);
            }

            Id = builder.ToString().Split(".").ToList().Last();
            Root = Path.Combine("/var/local/lib/isolate", Id, "box");
        }

        public static async Task<Box> GetBoxAsync()
        {
            await CleanUpBoxAsync();
            var builder = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = $"--cg -b {Id} --init",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.OutputDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.ErrorDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception($"E: Isolate init exited with code {process.ExitCode}.\n" + builder);
            }
            return new Box();
        }

        public static async Task CleanUpBoxAsync()
        {
            var builder = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = $"-b {Id} --cleanup",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.OutputDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.ErrorDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                throw new Exception($"E: Isolate cleanup exited with code {process.ExitCode}.\n" + builder);
            }
        }

        public async Task<int> ExecuteAsync(
            string command,
            string meta = "/dev/null",
            IEnumerable<string> env = null,
            IEnumerable<string> path = null,
            IEnumerable<string> bind = null,
            string chroot = null,
            int proc = 1,
            float time = 1.0f,
            int memory = 256000,
            int disk = 409600,
            int stack = 128000,
            string stdin = "/dev/null",
            string stdout = "/dev/null",
            string stderr = "/dev/null"
        )
        {
            var envOpt = "";
            if (env is not null)
            {
                envOpt = string.Join(' ', env.Select(e => $"-E {e}").ToList());
            }

            var pathOpt = "-E PATH=/bin:/usr/bin";
            if (path is not null)
            {
                pathOpt = "-E PATH=" + string.Join(':', path);
            }

            var bindOpt = "";
            if (bind is not null)
            {
                bindOpt = string.Join(' ', bind.Select(mp => $"-d {mp}").ToList());
            }

            var chrootOpt = "";
            if (chroot is not null)
            {
                chrootOpt = $"-c {chroot}";
            }

            var builder = new StringBuilder();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = $"--cg -b {Id} -s -M {meta}" +
                                $" {envOpt} {pathOpt} {bindOpt} {chrootOpt}" +
                                $" -i {stdin} -o {stdout} -r {stderr}" +
                                $" -p{proc} -f {disk} -k {stack} --cg-mem={memory}" +
                                $" --cg-timing -t {time} -x 0 -w {time + 3.0f}" +
                                $" --run -- {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.OutputDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.ErrorDataReceived += new DataReceivedEventHandler(
                delegate(object sender, DataReceivedEventArgs args) { builder.Append(args.Data); });
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            if (process.ExitCode == 0 || process.ExitCode == 1)
            {
                return process.ExitCode;
            }
            else
            {
                throw new Exception($"E: Isolate run exited with code {process.ExitCode}.\n" + builder);
            }
        }

        public async Task<string> ReadFileAsync(string file)
        {
            if (!File.Exists(Path.Combine(Root, file))) return string.Empty;
            var builder = new StringBuilder();
            var buffer = new char[1024];
            await using var stream = new FileStream(Path.Combine(Root, file), FileMode.Open);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            while (true)
            {
                var length = await reader.ReadBlockAsync(buffer, 0, 1024);
                if (length == 0) break;
                var result = new char[length];
                Array.Copy(buffer, result, length);
                builder.Append(new string(result));
            }
            return builder.ToString();
        }

        public async Task<Dictionary<string, string>> ReadDictAsync(string file)
        {
            if (!File.Exists(Path.Combine(Root, file))) return new();
            return (await File.ReadAllLinesAsync(Path.Combine(Root, file)))
                .Select(l => l.Split(':'))
                .Where(s => s.Length == 2)
                .ToDictionary(s => s[0], s => s[1]);
        }

        // This is a sealed class so we don't have to implement virtual disposers.
        public void Dispose()
        {
            CleanUpBoxAsync().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await CleanUpBoxAsync();
        }
    }
}