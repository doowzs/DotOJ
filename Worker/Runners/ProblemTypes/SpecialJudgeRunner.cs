using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Worker.Models;

namespace Worker.Runners.ProblemTypes
{
    public class SpecialJudgeRunner : PlainRunner
    {
        public SpecialJudgeRunner(Contest contest, Problem problem, Submission submission, IServiceProvider provider)
            : base(contest, problem, submission, provider)
        {
            Logger = provider.GetRequiredService<ILogger<SpecialJudgeRunner>>();

            if (problem.SpecialJudgeProgram == null)
            {
                throw new Exception($"Special judge program is null for Problem={problem.Id}.");
            }

            OnRunCompleteDelegate = OnRunCompleteImpl;
            InnerOnRunFailedDelegate = InnerOnRunFailedImpl;
        }

        private async Task<string> CreateSpjRunAsync(Run run)
        {
            var temp = Path.GetTempPath();
            var path = Path.Combine(temp, Submission.Id.ToString());
            var zip = Path.Combine(temp, "spj" + Submission.Id + ".zip");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (File.Exists(zip))
            {
                File.Delete(zip);
            }

            await using (var stream = new FileStream(zip, FileMode.Create))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                // Prepare testlib.h and backend configuration files.
                {
                    var pairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("testlib.h", "644"),
                        new KeyValuePair<string, string>("compile", "755"),
                        new KeyValuePair<string, string>("run", "755")
                    };
                    foreach (var pair in pairs)
                    {
                        var entry = archive.CreateEntry(pair.Key);
                        entry.ExternalAttributes |= Convert.ToInt32(pair.Value, 8) << 16;
                        await using var entryStream = entry.Open();
                        await using var content = new FileStream(Path.Combine("Resources", pair.Key),
                            FileMode.Open, FileAccess.Read);
                        await content.CopyToAsync(entryStream);
                    }
                }


                // Prepare spj.cc, input, output and answer files.
                {
                    var checkerEntry = archive.CreateEntry("checker.cpp");
                    checkerEntry.ExternalAttributes |= Convert.ToInt32("644", 8) << 16;
                    await using var checkerStream = checkerEntry.Open();
                    var checkerBytes = Convert.FromBase64String(Problem.SpecialJudgeProgram.Code);
                    await checkerStream.WriteAsync(checkerBytes);
                }
                {
                    var inputEntry = archive.CreateEntry("input");
                    inputEntry.ExternalAttributes |= Convert.ToInt32("644", 8) << 16;
                    await using var inputStream = inputEntry.Open();
                    if (run.Inline)
                    {
                        // Input in DB is Base64 encoded.
                        var inputBytes = Convert.FromBase64String(Problem.SampleCases[run.Index - 1].Input);
                        await inputStream.WriteAsync(inputBytes);
                    }
                    else
                    {
                        var inputFile = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(),
                            Problem.TestCases[run.Index - 1].Input);
                        await using var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(inputStream);
                    }
                }
                {
                    var answerEntry = archive.CreateEntry("answer");
                    answerEntry.ExternalAttributes |= Convert.ToInt32("644", 8) << 16;
                    await using var answerStream = answerEntry.Open();
                    if (run.Inline)
                    {
                        // Answer in DB is Base64 encoded.
                        var answerBytes = Convert.FromBase64String(Problem.SampleCases[run.Index - 1].Output);
                        await answerStream.WriteAsync(answerBytes);
                    }
                    else
                    {
                        var answerFile = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(),
                            Problem.TestCases[run.Index - 1].Output);
                        await using var fileStream = new FileStream(answerFile, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(answerStream);
                    }
                }
                {
                    // Output from RunnerResponse is Base64 encoded.
                    var outputEntry = archive.CreateEntry("output");
                    outputEntry.ExternalAttributes |= Convert.ToInt32("644", 8) << 16;
                    await using var outputStream = outputEntry.Open();
                    var outputBytes = Convert.FromBase64String(run.Stdout);
                    await outputStream.WriteAsync(outputBytes);
                }
            }

            var additional = Convert.ToBase64String(await File.ReadAllBytesAsync(zip));
            RunnerOptions options = new RunnerOptions(Problem, Submission, additional);

            var uri = Options.Value.Instance.Endpoint + "/submissions?base64_encoded=true";
            using var stringContent = new StringContent(JsonConvert.SerializeObject(options),
                Encoding.UTF8, MediaTypeNames.Application.Json);
            using var response = await Client.PostAsync(uri, stringContent);
            Directory.Delete(path, true); // Delete folder after creating a new run.
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"CreateSpjRun failed Submission={Submission.Id}" +
                                $" Index={run.Index} Status={response.StatusCode}");
                throw new Exception($"CreateRun API call failed with code {response.StatusCode}.");
            }

            var token = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            Logger.LogInformation($"CreateSpjRun succeed Submission={Submission.Id} " +
                                  (run.Inline ? $"SampleCase" : $"TestCase") + $"={run.Index} Token={token}");
            return token.Token;
        }

        private async Task OnRunCompleteImpl(Run run)
        {
            run.Token = await CreateSpjRunAsync(run);
            run.Verdict = Verdict.Running;
            await PollRunAsync(run);
            await DeleteRunAsync(run);
        }

        private Task InnerOnRunFailedImpl(Run run)
        {
            // Any failure in SPJ is considered Wrong Answer.
            run.Verdict = Verdict.WrongAnswer;
            return Task.CompletedTask;
        }
    }
}