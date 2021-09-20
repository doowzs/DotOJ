using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Shared.Models;
using Shared.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Worker.Runners.CheckPlagiarism
{
    public class PlagiarismChecker : JobRunnerBase<PlagiarismChecker>
    {
        public PlagiarismChecker(IServiceProvider provider) : base(provider)
        {
        }

        public override async Task<int> HandleJobRequest(JobRequestMessage message)
        {
            var plagiarism = await Context.Plagiarisms.FindAsync(message.TargetId);
            if (plagiarism is null || plagiarism.RequestVersion >= message.RequestVersion)
            {
                Logger.LogDebug($"IgnoreCheckPlagiarismMessage" +
                                $" PlagiarismId={message.TargetId}" +
                                $" RequestVersion={message.RequestVersion}");
                return 0;
            }
            else
            {
                plagiarism.RequestVersion = message.RequestVersion;
                plagiarism.CheckedBy = Options.Value.Name;
                Context.Update(plagiarism);
                await Context.SaveChangesAsync();
            }

            Logger.LogInformation($"CheckPlagiarism Id={plagiarism.Id} Problem={plagiarism.ProblemId}");
            var stopwatch = Stopwatch.StartNew();

            var submissions = await Context.Submissions
                .Where(s => s.ProblemId == plagiarism.ProblemId && s.Verdict == Verdict.Accepted)
                .Include(s => s.User)
                .ToListAsync();

            Dictionary<string, Tuple<string, List<Submission>>> groups = new()
            {
                { "c/c++", Tuple.Create("c_cpp", new List<Submission>()) },
                { "java11", Tuple.Create("java", new List<Submission>()) },
                { "python3", Tuple.Create("python", new List<Submission>()) },
                { "c#-1.2", Tuple.Create("csharp", new List<Submission>()) },
                { "text", Tuple.Create("others", new List<Submission>()) }
            };
            foreach (var submission in submissions)
            {
                switch (submission.Program.Language)
                {
                    case Language.C:
                    case Language.Cpp:
                        groups["c/c++"].Item2.Add(submission);
                        break;
                    case Language.Java:
                        groups["java11"].Item2.Add(submission);
                        break;
                    case Language.Python:
                        groups["python3"].Item2.Add(submission);
                        break;
                    case Language.CSharp:
                        groups["c#-1.2"].Item2.Add(submission);
                        break;
                    default:
                        groups["text"].Item2.Add(submission);
                        break;
                }
            }

            var root = Path.Combine(Options.Value.DataPath, "plagiarisms", plagiarism.Id.ToString());
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var log = Path.Combine(root, "log.txt");
            await using (var writer = new StreamWriter(log, false, Encoding.UTF8))
            {
                await writer.WriteLineAsync($"Plagiarism #{plagiarism.Id} for Problem #{plagiarism.ProblemId}\n" +
                                            $"Date:   {DateTime.Now.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC\n" +
                                            $"Worker: {Options.Value.Name}\n");
                await writer.FlushAsync();
                writer.Close();
            }

            var results = new List<PlagiarismResult>();
            foreach (var group in groups)
            {
                if (group.Value.Item2 != null && group.Value.Item2.Count > 0)
                {
                    Logger.LogInformation($"CheckPlagiarism Id={plagiarism.Id}" +
                                          $" Group={group.Key} TimeElapsed={stopwatch.Elapsed}");
                    var ok = await CheckPlagiarismInGroup(plagiarism, group.Key, group.Value.Item1, group.Value.Item2);
                    var result = new PlagiarismResult
                    {
                        Name = group.Key,
                        Count = group.Value.Item2.Count,
                        Path = ok ? group.Value.Item1 : null
                    };
                    results.Add(result);
                }
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Validate that the plagiarism is not touched by others since picking up.
                var fetched = await Context.Plagiarisms.FindAsync(plagiarism.Id);
                if (fetched.RequestVersion == message.RequestVersion && fetched.CheckedBy == Options.Value.Name)
                {
                    plagiarism.Results = results;
                    plagiarism.CheckedAt = DateTime.Now.ToUniversalTime();
                    Context.Update(plagiarism);
                    await Context.SaveChangesAsync();
                }

                scope.Complete();
            }

            stopwatch.Stop();
            Logger.LogInformation($"CheckPlagiarism Complete Id={plagiarism.Id}" +
                                  $" Problem={plagiarism.ProblemId} TimeElapsed={stopwatch.Elapsed}");
            return 1;
        }

        private async Task<bool> CheckPlagiarismInGroup
            (Plagiarism plagiarism, string groupName, string pathName, List<Submission> submissions)
        {
            var ok = true;

            var root = Path.Combine(Options.Value.DataPath, "plagiarisms", plagiarism.Id.ToString());
            var log = Path.Combine(root, "log.txt");
            var results = Path.Combine(root, pathName);
            var sources = Path.Combine(root, pathName + "_sources");
            Directory.CreateDirectory(results);
            Directory.CreateDirectory(sources);

            try
            {
                #region Write All accepted submissions into files

                var suffixes = submissions.Select(s => s.Program.GetSourceFileExtension()).Distinct().ToList();
                var dict = new Dictionary<string, int>();
                
                foreach (var submission in submissions)
                {
                    if (!dict.TryGetValue(submission.User.ContestantId, out var times)) times = 0;
                    dict[submission.User.ContestantId] = times + 1;
                    var filename = submission.User.ContestantId + "-" + dict[submission.User.ContestantId] + submission.Program.GetSourceFileExtension();
                    var fullPath = Path.Combine(sources, filename);
                    await using var stream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                    await stream.WriteAsync(Convert.FromBase64String(submission.Program.Code));
                    await stream.FlushAsync();
                    stream.Close();
                }

                #endregion

                #region Run jPlag to calculate similarities

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/java",
                        Arguments = $"-jar Resources/jplag/jplag.jar" +
                                    $" -vl -m 40% -l {groupName}" +
                                    $" -p {string.Join(',', suffixes)}" +
                                    $" -r {Path.Combine(results)}" +
                                    $" {Path.Combine(sources)}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                var stdout = new StringBuilder();
                var stderr = new StringBuilder();
                process.OutputDataReceived += (sender, e) => stdout.AppendLine(e.Data);
                process.ErrorDataReceived += (sender, e) => stderr.AppendLine(e.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                await using (var writer = new StreamWriter(log, true, Encoding.UTF8))
                {
                    var group = "Group: " + groupName;
                    var len = (38 - group.Length) / 2;
                    await writer.WriteLineAsync(new string('*', 40));
                    await writer.WriteLineAsync("*" + new string(' ', len) + group +
                                                new string(' ', 38 - group.Length - len) + "*");
                    await writer.WriteLineAsync(new string('*', 40) + "\n");
                    await writer.WriteLineAsync($"Command: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                    await writer.WriteLineAsync($"Stdout of process:\n" +
                                                $"{string.Concat(stdout.ToString().Split('\n').Select(s => "    " + s + "\n"))}");
                    if (stderr.Length > 0)
                    {
                        await writer.WriteLineAsync($"Stderr of process:\n" +
                                                    $"{string.Concat(stderr.ToString().Split('\n').Select(s => "    " + s + "\n"))}");
                    }

                    await writer.FlushAsync();
                    writer.Close();
                }

                if (process.ExitCode != 0)
                {
                    throw new Exception($"jplag exited with code {process.ExitCode}");
                }

                #endregion
            }
            catch (Exception e)
            {
                ok = false;
                if (submissions.Count >= 1)
                {
                    Logger.LogError($"CheckPlagiarismFail GroupName={groupName} Reason={e.Message}\n" +
                                    $"Stacktrace:\n{e.StackTrace}");
                }
            }

            Directory.Delete(sources, true);
            return ok;
        }
    }
}