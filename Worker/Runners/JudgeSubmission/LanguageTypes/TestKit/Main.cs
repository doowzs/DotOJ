using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.Archives.v2.TestKit;
using Shared.Configs;
using Shared.Models;
using Worker.Models;
using Worker.Runners.JudgeSubmission.LanguageTypes.Base;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private readonly Contest _contest;
        private readonly Problem _problem;
        private readonly Submission _submission;
        private readonly Box _box;
        private readonly ILogger<TestKitRunner> _logger;
        private readonly IOptions<WorkerConfig> _options;

        private string DataPath => _options.Value?.DataPath ?? "/data";
        private string Root => Box.Root;
        private string Jail => Path.Combine(Root, "jail");

        private string _kit = string.Empty;
        private Manifest _manifest = null;
        private readonly StringBuilder _message = new();

        public TestKitRunner
            (Contest contest, Problem problem, Submission submission, Box box, IServiceProvider provider)
        {
            _contest = contest;
            _problem = problem;
            _submission = submission;
            _box = box;
            _logger = provider.GetRequiredService<ILogger<TestKitRunner>>();
            _options = provider.GetRequiredService<IOptions<WorkerConfig>>();

            if (!Directory.Exists(Jail))
            {
                Directory.CreateDirectory(Jail);
            }
        }

        public async Task<JudgeResult> RunSubmissionAsync()
        {
            var extract = await ExtractAsync();
            if (extract is not null && extract.Verdict != Verdict.Accepted)
            {
                return extract;
            }

            var restore = await RestoreGitRepositoryAsync();
            if (restore is not null)
            {
                if (restore.Verdict != Verdict.Accepted)
                {
                    return restore;
                }
                _message.AppendLine(restore?.Message);
            }

            _kit = Path.Combine(_options.Value.DataPath, "tests", _problem.Id.ToString());
            await using (var stream = new FileStream(Path.Combine(_kit, "manifest.json"), FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream))
            {
                _manifest = JsonConvert.DeserializeObject<Manifest>(await reader.ReadToEndAsync());
                if (_manifest is null || !_manifest.Version.Equals("2"))
                {
                    throw new Exception("E: Failed to parse testkit (invalid version, not v2).");
                }
            }

            var verdict = Verdict.Accepted;
            var score = 0;
            await foreach (var result in RunStagesAsync())
            {
                if (verdict == Verdict.Accepted && result.Verdict != Verdict.Accepted)
                {
                    verdict = result.Verdict;
                }
                score += result.Score;
                if (result.Message is not null)
                {
                    _message.AppendLine(result.Message);
                }
            }

            var total = _manifest.Stages
                .Sum(stage => IsStudentInGroups(stage.Groups)
                    ? stage.Steps.Sum(step => IsStudentInGroups(step.Groups) ? step.Score : 0)
                    : 0);
            if (total == 0)
            {
                throw new Exception("E: Sum of score of all steps in this group is 0.");
            }

            return new JudgeResult
            {
                Verdict = verdict,
                FailedOn = verdict switch
                {
                    Verdict.Rejected => 0,
                    Verdict.Failed => 0,
                    Verdict.Accepted => null,
                    _ => 1
                },
                Time = null,
                Memory = null,
                Score = score * 100 / total,
                Message = _message.ToString()
            };
        }
    }
}