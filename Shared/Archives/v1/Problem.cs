using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Configs;
using Shared.Models;

namespace Shared.Archives.v1
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

                if (problem.HasSpecialJudge)
                {
                    var checkerEntry = archive.CreateEntry("checker.cpp");
                    await using var checkerStream = checkerEntry.Open();
                    await checkerStream.WriteAsync(Convert.FromBase64String(problem.SpecialJudgeProgram.Code));
                    checkerStream.Close();
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
                    var inputFile = Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString(), test.Input);
                    await using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    {
                        var inputEntry = archive.CreateEntry(Path.Combine("tests", test.Input));
                        await using var inputStream = inputEntry.Open();
                        await fileStream.CopyToAsync(inputStream);
                        inputStream.Close();
                    }

                    var outputFile = Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString(), test.Output);
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

        public static async Task<Problem> ParseAsync
            (int contestId, IFormFile file, IOptions<ApplicationConfig> options)
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var problem = new Problem
            {
                ContestId = contestId,
            };

            #region Validate archive version

            var metaEntry = archive.GetEntry("version");
            if (metaEntry == null)
            {
                throw new ValidationException("Metafile not found.");
            }

            await using (var metaStream = metaEntry.Open())
            using (var metaReader = new StreamReader(metaStream))
            {
                var metaString = await metaReader.ReadToEndAsync();
                if (!metaString.Equals("1"))
                {
                    throw new ValidationException("Invalid archive version.");
                }
            }

            #endregion

            #region Read config file

            var configEntry = archive.GetEntry("config.json");
            if (configEntry == null)
            {
                throw new ValidationException("Config file not found.");
            }

            await using (var configStream = configEntry.Open())
            using (var configReader = new StreamReader(configStream))
            {
                var configString = await configReader.ReadToEndAsync();
                var config = JsonConvert.DeserializeObject<ProblemConfig>(configString);

                problem.Title = config.Title;
                problem.TimeLimit = config.TimeLimit;
                problem.MemoryLimit = config.MemoryLimit;
                problem.HasSpecialJudge = config.HasSpecialJudge;
            }

            #endregion

            #region Read special judge checker if avilable

            if (problem.HasSpecialJudge)
            {
                var checkerEntry = archive.GetEntry("checker.cpp");
                if (checkerEntry == null)
                {
                    throw new ValidationException("Special judge checker not found.");
                }

                await using (var checkerStream = checkerEntry.Open())
                using (var checkerReader = new StreamReader(checkerStream))
                {
                    var checkerString = await checkerReader.ReadToEndAsync();
                    problem.SpecialJudgeProgram = new Program
                    {
                        Language = Language.Cpp,
                        Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(checkerString))
                    };
                }
            }

            #endregion

            #region Read problem description files

            var descriptionEntry = archive.GetEntry("problem/description.md");
            if (descriptionEntry == null)
            {
                throw new ValidationException("Problem description not found.");
            }

            await using (var descriptionStream = descriptionEntry.Open())
            using (var descriptionReader = new StreamReader(descriptionStream))
            {
                problem.Description = await descriptionReader.ReadToEndAsync();
            }

            var inputFormatEntry = archive.GetEntry("problem/input-format.md");
            if (inputFormatEntry == null)
            {
                throw new ValidationException("Problem input format not found.");
            }

            await using (var inputFormatStream = inputFormatEntry.Open())
            using (var inputFormatReader = new StreamReader(inputFormatStream))
            {
                problem.InputFormat = await inputFormatReader.ReadToEndAsync();
            }

            var outputFormatEntry = archive.GetEntry("problem/output-format.md");
            if (outputFormatEntry == null)
            {
                throw new ValidationException("Problem output format not found.");
            }

            await using (var outputFormatStream = outputFormatEntry.Open())
            using (var outputFormatReader = new StreamReader(outputFormatStream))
            {
                problem.OutputFormat = await outputFormatReader.ReadToEndAsync();
            }

            var footnoteEntry = archive.GetEntry("problem/footnote.md");
            if (footnoteEntry != null)
            {
                await using var footnoteStream = footnoteEntry.Open();
                using var footnoteReader = new StreamReader(footnoteStream);
                problem.FootNote = await footnoteReader.ReadToEndAsync();
            }

            #endregion

            #region Read sample cases

            problem.SampleCases = new List<TestCase>();
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("samples/") && Path.GetExtension(entry.FullName) == ".in")
                {
                    var inputEntry = entry;
                    var outputEntry = archive.GetEntry(Path.ChangeExtension(entry.FullName, ".out"));
                    if (outputEntry == null)
                    {
                        continue;
                    }

                    await using var inputStream = inputEntry.Open();
                    await using var outputStream = outputEntry.Open();
                    using var inputReader = new StreamReader(inputStream);
                    using var outputReader = new StreamReader(outputStream);
                    var inputBytes = Encoding.UTF8.GetBytes(await inputReader.ReadToEndAsync());
                    var outputBytes = Encoding.UTF8.GetBytes(await outputReader.ReadToEndAsync());
                    problem.SampleCases.Add(new TestCase
                    {
                        Input = Convert.ToBase64String(inputBytes),
                        Output = Convert.ToBase64String(outputBytes)
                    });
                }
            }

            #endregion

            // Cannot extract now because we don't have an ID.
            problem.TestCases = new List<TestCase>();

            return problem;
        }

        public static async Task ExtractTestCasesAsync
            (Problem problem, IFormFile file, string prefix, IOptions<ApplicationConfig> options)
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var inputs = new HashSet<string>();
            var path = Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString());

            #region Find valid test case pairs

            // Traverse all files in zip archive and get input filenames.
            foreach (var entry in archive.Entries)
            {
                var filename = entry.FullName;
                var extension = Path.GetExtension(entry.Name);
                if (filename.StartsWith(prefix) && extension.Equals(".in") &&
                    archive.GetEntry(Path.ChangeExtension(filename, ".out")) != null)
                {
                    inputs.Add(filename);
                }
            }

            #endregion

            #region Create or clear current test case folder

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var dir = new DirectoryInfo(path);
            foreach (var f in dir.EnumerateFiles())
            {
                f.Delete();
            }

            foreach (var d in dir.EnumerateDirectories())
            {
                d.Delete(true);
            }

            #endregion

            #region Extract test case files into test case folder

            foreach (var input in inputs)
            {
                var output = Path.ChangeExtension(input, ".out");
                var list = new List<string> {input, output};
                foreach (var filename in list)
                {
                    var entry = archive.GetEntry(filename);
                    if (entry == null)
                    {
                        throw new Exception($"Entry for {filename} is null.");
                    }

                    var dest = Path.Combine(path, filename.Substring(prefix.Length));
                    var folder = Path.GetDirectoryName(dest);
                    if (folder != null && !Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    await using var fs = new FileStream(dest, FileMode.Create);
                    var srcStream = entry.Open();
                    await srcStream.CopyToAsync(fs);
                    srcStream.Close();
                }
            }

            #endregion

            problem.TestCases = inputs.Select(input => new TestCase
            {
                Input = input.Substring(prefix.Length),
                Output = Path.ChangeExtension(input.Substring(prefix.Length), ".out")
            }).ToList();
        }
    }
}