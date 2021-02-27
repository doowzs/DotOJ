using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Configs;
using Shared.Models;

namespace Shared.Archives.v2.Problems
{
    public static class TestKitLabProblemArchive
    {
        public static async Task<byte[]> CreateAsync(Problem problem, IOptions<ApplicationConfig> options)
        {
            await using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var metaEntry = archive.CreateEntry("version");
                await using var metaStream = metaEntry.Open();
                await metaStream.WriteAsync(Encoding.UTF8.GetBytes("2"));
                metaStream.Close();

                var config = new ProblemConfig(problem);
                var configString = JsonConvert.SerializeObject(config);
                var configEntry = archive.CreateEntry("config.json");
                await using var configStream = configEntry.Open();
                await configStream.WriteAsync(Encoding.UTF8.GetBytes(configString));
                configStream.Close();

                var descriptionEntry = archive.CreateEntry("problem/description.md");
                await using var descriptionStream = descriptionEntry.Open();
                await descriptionStream.WriteAsync(Encoding.UTF8.GetBytes(problem.Description));
                descriptionStream.Close();

                foreach (var dataFile in problem.TestCases.Select(tc => tc.Input))
                {
                    var dataEntry = archive.CreateEntry(Path.Combine("tests", dataFile).Replace('\\', '/'));
                    var fullPath = Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString(), dataFile);
                    await using var dataStream = dataEntry.Open();
                    await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(dataStream);
                    dataStream.Close();
                    fileStream.Close();
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
                if (!metaString.Equals("2"))
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
                problem.Type = ProblemType.TestKitLab;
                problem.TimeLimit = 1000;
                problem.MemoryLimit = 256000;
                problem.HasSpecialJudge = false;
                problem.HasHacking = false;
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

            problem.InputFormat = string.Empty;
            problem.OutputFormat = string.Empty;
            problem.FootNote = null;

            #endregion

            // Test kit lab problems have no sample cases.
            problem.SampleCases = new List<TestCase>();

            // Cannot extract now because we don't have an ID.
            problem.TestCases = new List<TestCase>();

            return problem;
        }

        public static async Task ExtractTestKitAsync
            (Problem problem, IFormFile file, IOptions<ApplicationConfig> options, string prefix = "tests/")
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var dataFiles = new List<string>();
            var path = Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString());

            #region List all files in test kit

            foreach (var entry in archive.Entries)
            {
                var filename = entry.FullName;
                if (filename.StartsWith(prefix))
                {
                    dataFiles.Add(filename);
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

            #region Extract test kit files into test case folder

            foreach (var filename in dataFiles)
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

            #endregion

            problem.TestCases = dataFiles.Select(dataFile => new TestCase
            {
                Input = dataFile.Substring(prefix.Length),
                Output = string.Empty
            }).ToList();
        }
    }
}