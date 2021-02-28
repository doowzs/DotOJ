using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Configs;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.ProblemTypes
{
    public class TestKitLabRunner : PlainRunner
    {
        public TestKitLabRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
            : base(contest, problem, submission, box, provider)
        {
            var options = provider.GetRequiredService<IOptions<WorkerConfig>>();
            if (!File.Exists(Path.Combine(options.Value.DataPath, "tests", problem.Id.ToString(), "manifest.json")))
            {
                throw new Exception($"TestKit manifest.json does not exist for Problem={problem.Id}.");
            }
        }
    }
}