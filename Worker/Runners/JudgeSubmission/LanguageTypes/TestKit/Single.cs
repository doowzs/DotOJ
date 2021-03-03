using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Archives.v2.TestKit;
using Shared.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private async Task<Verdict> RunSingleAsync([NotNull] Config config, string affix = "")
        {
            var meta = Path.Combine(Root, "meta{affix}");
            var stdin = string.IsNullOrEmpty(config.Input) ? "/dev/null" : config.Input;
            var stdout = $"stdout{affix}";
            var stderr = $"stderr{affix}";
            var time = config.Time ?? _manifest.Default?.Time ?? 5000;
            var memory = config.Memory ?? _manifest.Default?.Memory ?? 512000;
            var thread = config.Thread ?? _manifest.Default?.Thread ?? 5;
            var disk = config.Disk ?? _manifest.Default?.Disk ?? 409600;

            if (config.Files != null)
            {
                foreach (var pair in config.Files)
                {
                    var file = Path.Combine(_kit, "files", pair.Key);
                    var dest = Path.Combine(Jail, pair.Value);
                    File.Copy(file, dest, true);
                }
            }

            var result = await _box.ExecuteAsync(
                config.Command,
                meta: meta,
                bind: new[] {"/etc"},
                chroot: "jail",
                stdin: stdin,
                stdout: stdout,
                stderr: stderr,
                proc: thread,
                disk: disk,
                time: (float) time / 1000,
                memory: memory
            );

            if (result == 0)
            {
                return Verdict.Accepted;
            }

            var dict = await _box.ReadDictAsync(meta);
            if (dict.ContainsKey("status"))
            {
                switch (dict["status"])
                {
                    case "SG":
                        if (dict.ContainsKey("exitsig"))
                        {
                            if (dict["exitsig"].Equals("25"))
                            {
                                return Verdict.WrongAnswer;
                            }

                            goto case "RE"; // fall through
                        }

                        break;
                    case "RE":
                        return dict.ContainsKey("cg-oom-killed")
                            ? Verdict.MemoryLimitExceeded
                            : Verdict.RuntimeError;
                    case "TO":
                        return Verdict.TimeLimitExceeded;
                    case "XX":
                        throw new Exception("Isolate internal error XX in meta file.");
                }
            }

            return Verdict.RuntimeError;
        }
    }
}