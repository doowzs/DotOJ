using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Data.Configs;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Worker.Models;

namespace Worker.Runners.LanguageTypes
{
    public abstract class LanguageRunnerBase
    {
        protected readonly Contest Contest;
        protected readonly Problem Problem;
        protected readonly Submission Submission;

        protected float TimeLimit => Problem.TimeLimit *
            LanguageOptions.LanguageOptionsDict[Submission.Program.Language.GetValueOrDefault()].TimeFactor / 1000;

        protected int MemoryLimit => Problem.MemoryLimit;

        protected readonly ApplicationDbContext Context;
        protected readonly IOptions<JudgingConfig> Options;
        protected ILogger Logger;
        protected string Box;

        public Func<Contest, Problem, Submission, Task<JudgeResult>> BeforeStartDelegate = null;
        public Func<Contest, Problem, Submission, bool, Task<JudgeResult>> BeforeTestGroupDelegate = null;
        public Func<Contest, Problem, Submission, Run, Task<JudgeResult>> OnRunFailedDelegate = null;

        protected LanguageRunnerBase(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
        {
            Contest = contest;
            Problem = problem;
            Submission = submission;

            Context = provider.GetRequiredService<ApplicationDbContext>();
            Options = provider.GetRequiredService<IOptions<JudgingConfig>>();
            Logger = provider.GetRequiredService<ILogger<LanguageRunnerBase>>();
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            await InitAsync();
            var result = await InnerRunSubmissionAsync();
            //await CleanupAsync();
            return result;
        }

        private async Task InitAsync()
        {
            var cleaner = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = "--cg --cleanup"
                }
            };
            cleaner.Start();
            cleaner.WaitForExit();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = "--cg --init",
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            process.WaitForExit();
            Box = Path.Combine((await process.StandardOutput.ReadToEndAsync()).Trim(), "box");
            Logger.LogInformation($"Box created with Path={Box}.");
        }

        protected virtual Task<JudgeResult> CompileAsync()
        {
            return Task.FromResult<JudgeResult>(null);
        }

        private async Task PrepareTestCaseAsync(bool inline, TestCase testCase)
        {
            var file = Path.Combine(Box, "input");
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                if (inline)
                {
                    await using var inputWriter = new StreamWriter(stream);
                    await inputWriter.WriteAsync(Encoding.UTF8.GetString(Convert.FromBase64String(testCase.Input)));
                }
                else
                {
                    var dataFile = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Output);
                    await using var dataStream = new FileStream(dataFile, FileMode.Open, FileAccess.Read);
                    await dataStream.CopyToAsync(stream);
                }
            }
        }

        protected virtual Task ExecuteProgramAsync(string meta, int bytes)
        {
            return Task.CompletedTask;
        }

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
                Message = null
            };

            var meta = Path.Combine(Box, "meta");
            var bytes = 0;
            if (inline)
            {
                bytes = testCase.Output.Length * 3 / 4;
            }
            else
            {
                var answer = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Output);
                var info = new FileInfo(answer);
                bytes = (int) info.Length;
            }

            await PrepareTestCaseAsync(inline, testCase);
            await ExecuteProgramAsync(meta, bytes);

            var output = Path.Combine(Box, "output");
            await using (var stream = new FileStream(output, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                run.Stdout = await reader.ReadToEndAsync();
            }

            var stderr = Path.Combine(Box, "stderr");
            await using (var stream = new FileStream(stderr, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                run.Stderr = await reader.ReadToEndAsync();
            }

            var dict = new Dictionary<string, string>();
            var lines = await File.ReadAllLinesAsync(meta);
            foreach (var line in lines)
            {
                var idx = line.IndexOf(":");
                if (idx >= 0)
                {
                    var key = line.Substring(0, idx);
                    var value = line.Substring(idx + 1);
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, value);
                    }
                }
            }

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
                                run.Message = $"Killed by signal {dict["exitsig"]}.";
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
                        break;
                    case "XX":
                        throw new Exception("Isolate internal error XX in meta file.");
                }
            }

            if (float.TryParse(dict["time"], out var time))
            {
                run.Time = (int) (time * 1000);
            }

            if (int.TryParse(dict["cg-mem"], out var memory))
            {
                run.Memory = memory;
            }

            return run;
        }

        private async Task<bool> CheckTestCaseOutputAsync(Run run, bool inline, TestCase testCase)
        {
            if (run.Verdict > Verdict.Accepted)
            {
                return true; // do not change a failed state
            }

            string output = run.Stdout.TrimEnd();
            string answer = null;
            if (inline)
            {
                answer = Encoding.UTF8.GetString(Convert.FromBase64String(testCase.Output)).TrimEnd();
            }
            else
            {
                var file = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Output);
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                answer = await reader.ReadToEndAsync();
            }

            return output.Equals(answer);
        }

        private async Task<JudgeResult> InnerRunSubmissionAsync()
        {
            JudgeResult result = null;
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

            Submission.Verdict = Verdict.Running;
            Context.Submissions.Update(Submission);
            await Context.SaveChangesAsync();

            stopWatch.Start();
            result = await CompileAsync();
            stopWatch.Stop();
            Logger.LogInformation($"Compile complete TimeElapsed={stopWatch.Elapsed}.");
            if (result != null)
            {
                return result;
            }

            var runs = new List<Run>();
            var testCasesPairList = new List<KeyValuePair<List<TestCase>, bool>>
            {
                new KeyValuePair<List<TestCase>, bool>(Problem.SampleCases, true),
                new KeyValuePair<List<TestCase>, bool>(Problem.TestCases, false)
            };
            int count = 0, total = Problem.SampleCases.Count + Problem.TestCases.Count;
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
                    if (run.Verdict > Verdict.Accepted)
                    {
                        Submission.Verdict = run.Verdict;
                        Submission.FailedOn = run.Index;
                        Context.Submissions.Update(Submission);

                        if (OnRunFailedDelegate != null)
                        {
                            result = await OnRunFailedDelegate.Invoke(Contest, Problem, Submission, run);
                            // Do not return immediately, we will save submission data on next lines.
                        }
                    }

                    Submission.Progress = ++count * 100 / total;
                    await Context.SaveChangesAsync();
                    if (result != null)
                    {
                        return result;
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
                Verdict = Submission.Verdict == Verdict.Running ? Verdict.Accepted : Submission.Verdict,
                Time = time,
                Memory = memory,
                FailedOn = failed?.Index,
                Score = count * 100 / total,
                Message = ""
            };
        }

        private async Task CleanupAsync()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = "--cleanup",
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            process.WaitForExit();
            Box = Path.Combine(await process.StandardOutput.ReadToEndAsync(), "box");
        }
    }
}