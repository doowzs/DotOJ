using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Data.Archives.v1
{
    public class ProblemArchive
    {
        private class ProblemConfig
        {
            public string Title { get; set; }
            public int TimeLimit { get; set; }
            public int MemoryLimit { get; set; }
            public bool HasSpecialJudge { get; set; }

            public ProblemConfig()
            {
            }

            public ProblemConfig(Problem problem)
            {
                Title = problem.Title;
                TimeLimit = problem.TimeLimit;
                MemoryLimit = problem.MemoryLimit;
                HasSpecialJudge = problem.HasSpecialJudge;
            }
        }

        public static async Task<byte[]> CreateAsync(Problem problem, IOptions<ApplicationConfig> options)
        {
            await using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var metaEntry = archive.CreateEntry("version");
                await using var metaStream = metaEntry.Open();
                await metaStream.WriteAsync(Encoding.UTF8.GetBytes("1"));
                metaStream.Close();

                var config = new ProblemConfig(problem);
                var configString = JsonConvert.SerializeObject(config);
                var configEntry = archive.CreateEntry("config.json");
                await using var configStream = configEntry.Open();
                await configStream.WriteAsync(Encoding.UTF8.GetBytes(configString));
                configStream.Close();

                var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("problem/description.md", problem.Description),
                    new KeyValuePair<string, string>("problem/input-format.md", problem.InputFormat),
                    new KeyValuePair<string, string>("problem/output-format.md", problem.OutputFormat),
                    new KeyValuePair<string, string>("problem/footnote.md", problem.FootNote)
                };
                foreach (var pair in pairs)
                {
                    if (pair.Value != null)
                    {
                        var fileEntry = archive.CreateEntry(pair.Key);
                        await using var fileStream = fileEntry.Open();
                        await fileStream.WriteAsync(Encoding.UTF8.GetBytes(pair.Value));
                        fileStream.Close();
                    }
                }

                int index = 0;
                foreach (var sample in problem.SampleCases)
                {
                    ++index;
                    var inputEntry = archive.CreateEntry(Path.Combine("samples", index + ".in"));
                    await using var inputStream = inputEntry.Open();
                    await inputStream.WriteAsync(Convert.FromBase64String(sample.Input));
                    inputStream.Close();

                    var outputEntry = archive.CreateEntry(Path.Combine("samples", index + ".out"));
                    await using var outputStream = outputEntry.Open();
                    await outputStream.WriteAsync(Convert.FromBase64String(sample.Output));
                    outputStream.Close();
                }

                foreach (var test in problem.TestCases)
                {
                    var inputFile = Path.Combine(options.Value.DataPath, problem.Id.ToString(), test.Input);
                    await using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    {
                        var inputEntry = archive.CreateEntry(Path.Combine("tests", test.Input));
                        await using var inputStream = inputEntry.Open();
                        await fileStream.CopyToAsync(inputStream);
                        inputStream.Close();
                    }

                    var outputFile = Path.Combine(options.Value.DataPath, problem.Id.ToString(), test.Output);
                    await using (var fileStream = new FileStream(outputFile, FileMode.Open, FileAccess.Read))
                    {
                        var outputEntry = archive.CreateEntry(Path.Combine("tests", test.Output));
                        await using var outputStream = outputEntry.Open();
                        await fileStream.CopyToAsync(outputStream);
                        outputStream.Close();
                    }
                }
            }

            // ZipArchive must be disposed before getting bytes of zip file.
            // See https://stackoverflow.com/questions/40175391/invalid-zip-file-after-creating-it
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream.ToArray();
        }
    }
}