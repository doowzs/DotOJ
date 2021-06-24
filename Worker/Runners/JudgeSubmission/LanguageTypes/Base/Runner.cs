using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Configs;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.Base
{
    public abstract partial class Runner
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;

        protected float TimeLimit => Problem.TimeLimit *
            LanguageOptions.LanguageOptionsDict[Submission.Program.Language.GetValueOrDefault()].TimeFactor / 1000;

        protected int MemoryLimit => Problem.MemoryLimit;

        private readonly IOptions<WorkerConfig> _options;
        protected readonly Box Box;
        protected ILogger Logger;
        protected string Root => Box.Root;
        protected string Jail => Path.Combine(Root, "jail");

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        protected Runner(Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;
            Box = box;
            _options = provider.GetRequiredService<IOptions<WorkerConfig>>();
            Logger = provider.GetRequiredService<ILogger<Runner>>();
            
            if (!Directory.Exists(Jail))
            {
                Directory.CreateDirectory(Jail);
            }
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            JudgeResult result;
            Stopwatch stopWatch = new Stopwatch();
            if (Problem.TestCases.Count <= 0)
            {
                return JudgeResult.NoTestCaseFailure;
            }

            if (BeforeStartDelegate != null)
            {
                result = await BeforeStartDelegate.Invoke(Contest, Problem, Submission);
                if (result != null)
                {
                    return result;
                }
            }

            if (!await CompileAsync())
            {
                var output = await Box.ReadFileAsync("compiler_output");
                if (output.Length > 4096)
                {
                    output = output.Substring(0, 4096)
                             + "\n*** Output trimmed due to excessive length of 4096 characters. ***";
                }
                return new JudgeResult
                {
                    Verdict = Verdict.CompilationError,
                    Time = null,
                    Memory = null,
                    FailedOn = 0,
                    Score = 0,
                    Message = output
                };
            }

            if (Submission.Program.Input != null)
            {
                stopWatch.Reset();
                stopWatch.Start();

                var test = new TestCase
                {
                    Input = Submission.Program.Input,
                    Output = new string(' ', 5464) // 4096 chars
                };
                var run = await RunTestCaseAsync(true, 0, test);
                var ok = run.Verdict == Verdict.Accepted;
                
                stopWatch.Stop();
                Logger.LogInformation($"SelfTest OK Verdict={run.Verdict} Time={run.Time} Memory={run.Memory}" +
                                      $" TimeElapsed={stopWatch.Elapsed}");
                
                if (run.Stdout.Length > 4096)
                {
                    run.Stdout = run.Stdout.Substring(0, 4096)
                                 + "\n*** Output trimmed due to excessive length of 4096 characters. ***";
                }

                return new JudgeResult
                {
                    Verdict = ok ? Verdict.CustomInputOk : run.Verdict,
                    Time = run.Time,
                    Memory = run.Memory,
                    FailedOn = null,
                    Score = 0,
                    Message = $"Input:\n{Encoding.UTF8.GetString(Convert.FromBase64String(Submission.Program.Input))}\n\n" +
                              (run.Verdict == Verdict.Accepted ? $"Output:\n{run.Stdout}" : $"Error: {run.Verdict}")
                };
            }

            if (Problem.HasSpecialJudge)
            {
                stopWatch.Reset();
                stopWatch.Start();
                await PrepareCheckerAsync();
                stopWatch.Stop();
                Logger.LogInformation($"Compile checker complete TimeElapsed={stopWatch.Elapsed}.");
            }

            var runs = new List<Run>();
            var testCasesPairList = new List<KeyValuePair<List<TestCase>, bool>>
            {
                new KeyValuePair<List<TestCase>, bool>(Problem.SampleCases, true),
                new KeyValuePair<List<TestCase>, bool>(Problem.TestCases, false)
            };
            int count = 0, total = 0;
            foreach (var pair in testCasesPairList)
            {
                int index = 0;
                var testCases = pair.Key;
                var inline = pair.Value;

                if (BeforeTestGroupDelegate != null)
                {
                    result = await BeforeTestGroupDelegate.Invoke(Contest, Problem, Submission, inline);
                    if (result != null)
                    {
                        return result;
                    }
                }

                foreach (var testCase in testCases)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    var run = await RunTestCaseAsync(inline, ++index, testCase);
                    if (!await CheckTestCaseOutputAsync(run, inline, testCase))
                    {
                        run.Verdict = Verdict.WrongAnswer;
                    }

                    stopWatch.Stop();
                    Logger.LogInformation((inline ? "SampleCase" : "TestCase") +
                                          $" #{index} OK Verdict={run.Verdict} Time={run.Time} Memory={run.Memory}" +
                                          $" TimeElapsed={stopWatch.Elapsed}");


                    runs.Add(run);
                    if (run.Verdict > Verdict.Accepted && OnRunFailedDelegate != null)
                    {
                        result = await OnRunFailedDelegate.Invoke(Contest, Problem, Submission, run);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            count = runs.Count(r => r.Inline == false && r.Verdict == Verdict.Accepted);
            total = Problem.TestCases.Count;
            int time = 0, memory = 0;
            var failed = runs.FirstOrDefault(r => r.Verdict > Verdict.Accepted);

            foreach (var run in runs)
            {
                if (run.Time.HasValue)
                {
                    time = Math.Max(time, run.Time.Value);
                }

                if (run.Memory.HasValue)
                {
                    memory = Math.Max(memory, run.Memory.Value);
                }
            }

            return new JudgeResult
            {
                // If there was any failure, submission's verdict will be changed from Running.
                Verdict = failed?.Verdict ?? Verdict.Accepted,
                Time = time,
                Memory = memory,
                FailedOn = failed?.Index,
                Score = count * 100 / total,
                Message = string.Empty
            };
        }

        protected abstract Task<bool> CompileAsync();

        private async Task PrepareCheckerAsync()
        {
            // Check if a pre-compiled binary checker exists.
            var binary = Path.Combine(_options.Value.DataPath, "tests", Problem.Id.ToString(), "checker");
            if (File.Exists(binary) && File.GetLastWriteTimeUtc(binary) > Problem.UpdatedAt)
            {
                File.Copy(binary, Path.Combine(Root, "checker"));
                return;
            }

            File.Copy("Resources/testlib/testlib.h", Path.Combine(Root, "testlib.h"));
            var checker = Path.Combine(Root, "checker.cpp");
            await using (var stream = new FileStream(checker, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(Convert.FromBase64String(Problem.SpecialJudgeProgram.Code));
            }

            var options = LanguageOptions.LanguageOptionsDict[Language.Cpp].CompilerOptions;
            var exitCode = await Box.ExecuteAsync(
                $"/usr/bin/g++ {options} -o checker checker.cpp",
                bind: new[] {"/etc"},
                stderr: "compiler_output",
                proc: 120,
                time: 15.0f,
                memory: 512000
            );
            if (exitCode != 0)
            {
                var message = await Box.ReadFileAsync("compiler_output");
                if (message.Length > 4096)
                {
                    message = message.Substring(0, 4096)
                              + "\n*** Output trimmed due to excessive length of 4096 characters. ***";
                }
                throw new Exception($"Prepare checker error ExitCode={exitCode}.\n{message}");
            }

            // Cache the binary file for later usage.
            try
            {
                File.Copy(Path.Combine(Root, "checker"), binary, true);
            }
            catch (Exception)
            {
                // Do nothing on a failed copy.
            }
        }

        private async Task PrepareTestCaseAsync(bool inline, TestCase testCase)
        {
            var file = Path.Combine(Jail, "input");
            await using var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            if (inline)
            {
                await using var inputWriter = new StreamWriter(stream);
                await inputWriter.WriteAsync(Encoding.UTF8.GetString(Convert.FromBase64String(testCase.Input)));
            }
            else
            {
                var dataFile = Path.Combine(_options.Value.DataPath, "tests", Problem.Id.ToString(), testCase.Input);
                await using var dataStream = new FileStream(dataFile, FileMode.Open, FileAccess.Read);
                await dataStream.CopyToAsync(stream);
            }
        }

        protected abstract Task ExecuteProgramAsync(string meta, int bytes);

        private async Task<Run> RunTestCaseAsync(bool inline, int index, TestCase testCase)
        {
            var run = new Run
            {
                Check = false,
                Inline = inline,
                Index = index,
                Stdout = "",
                Stderr = "",
                Verdict = Verdict.Accepted,
                Time = null,
                Memory = null,
                Message = ""
            };

            var meta = Path.Combine(Root, "meta");
            var bytes = 0;
            if (inline)
            {
                bytes = testCase.Output.Length * 3 / 4;
            }
            else
            {
                var answer = Path.Combine(_options.Value.DataPath, "tests", Problem.Id.ToString(), testCase.Output);
                var info = new FileInfo(answer);
                bytes = (int) info.Length;
            }

            await PrepareTestCaseAsync(inline, testCase);
            await ExecuteProgramAsync(meta, Math.Max(bytes * 2, 10 * 1024 * 1024));

            var output = Path.Combine(Jail, "output");
            await using (var stream = new FileStream(output, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                run.Stdout = await reader.ReadToEndAsync();
            }

            var stderr = Path.Combine(Jail, "stderr");
            await using (var stream = new FileStream(stderr, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                run.Stderr = await reader.ReadToEndAsync();
            }

            var dict = await Box.ReadDictAsync(meta);
            if (dict.ContainsKey("status"))
            {
                switch (dict["status"])
                {
                    case "SG":
                        if (dict.ContainsKey("exitsig"))
                        {
                            if (dict["exitsig"].Equals("25"))
                            {
                                run.Verdict = Verdict.WrongAnswer;
                                run.Message = $"Killed by signal {dict["exitsig"]} (output longer than 2x answer).";
                            }
                            else
                            {
                                if (dict["exitsig"].Equals("11"))
                                {
                                    run.Message = $"Killed by signal {dict["exitsig"]} (SIGSEGV).";
                                }
                                else
                                {
                                    run.Message = $"Killed by signal {dict["exitsig"]}.";
                                }

                                goto case "RE"; // fall through
                            }
                        }

                        break;
                    case "RE":
                        run.Verdict = dict.ContainsKey("cg-oom-killed")
                            ? Verdict.MemoryLimitExceeded
                            : Verdict.RuntimeError;
                        break;
                    case "TO":
                        run.Verdict = Verdict.TimeLimitExceeded;
                        if (dict.ContainsKey("time-wall") && float.TryParse(dict["time-wall"], out var wallTime))
                        {
                            run.Time = (int) (Math.Min(wallTime, TimeLimit) * 1000);
                        }

                        break;
                    case "XX":
                        throw new Exception("Isolate internal error XX in meta file.");
                }
            }

            if (!run.Time.HasValue && float.TryParse(dict["time"], out var time))
            {
                run.Time = (int) (Math.Min(time, TimeLimit) * 1000);
            }

            if (int.TryParse(dict["cg-mem"], out var memory))
            {
                run.Memory = Math.Min(memory, MemoryLimit);
            }

            return run;
        }
    }
}