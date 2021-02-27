using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared.Models;

namespace Shared.Archives.v2.Problems
{
    public class ProblemConfig
    {
        public ProblemType Type { get; set; }
        public string Title { get; set; }
        public int? TimeLimit { get; set; }
        public int? MemoryLimit { get; set; }
        public bool? HasSpecialJudge { get; set; }

        public ProblemConfig()
        {
        }

        public ProblemConfig(Problem problem)
        {
            Type = problem.Type;
            Title = problem.Title;
            TimeLimit = problem.Type switch
            {
                ProblemType.Ordinary => problem.TimeLimit,
                ProblemType.TestKitLab => null,
                _ => throw new ArgumentOutOfRangeException()
            };
            MemoryLimit = problem.Type switch
            {
                ProblemType.Ordinary => problem.MemoryLimit,
                ProblemType.TestKitLab => null,
                _ => throw new ArgumentOutOfRangeException()
            };
            HasSpecialJudge = problem.Type switch
            {
                ProblemType.Ordinary => problem.HasSpecialJudge,
                ProblemType.TestKitLab => null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task<ProblemType> PeekProblemTypeAsync(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var configEntry = archive.GetEntry("config.json");
            if (configEntry == null)
            {
                throw new ValidationException("Config file not found.");
            }

            await using var configStream = configEntry.Open();
            using var configReader = new StreamReader(configStream);
            var configString = await configReader.ReadToEndAsync();
            var config = JsonConvert.DeserializeObject<ProblemConfig>(configString);
            return config.Type;
        }
    }
}