using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Data.Configs;
using Data.Generics;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Worker.Models;

namespace Worker.Runners.Modes
{
    public class PracticeModeSubmissionRunner : ModeSubmissionRunnerBase<PracticeModeSubmissionRunner>
    {
        private const int JudgeTimeLimit = 300;

        public PracticeModeSubmissionRunner(IServiceProvider provider) : base(provider)
        {
        }

        public override async Task<Result> RunAsync(Submission submission, Problem problem)
        {
            submission.Verdict = Verdict.InQueue;
            submission.FailedOn = null;
            Context.Submissions.Update(submission);
            await Context.SaveChangesAsync();

            var runInfos = await CreateRuns(submission, problem);
            if (runInfos.IsNullOrEmpty())
            {
                return new Result
                {
                    Verdict = Verdict.Failed,
                    FailedOn = null, Score = 0,
                    Message = "No test cases available."
                };
            }

            for (int i = 0; i < JudgeTimeLimit; ++i)
            {
                await Task.Delay(1000);

                var result = await PollRuns(submission, runInfos);
                if (result != null)
                {
                    // Fix time and memory to be no larger than limit.
                    var timeFactor = RunnerLanguageOptions
                        .LanguageOptionsDict[submission.Program.Language.GetValueOrDefault()].timeFactor;
                    result.Time = Math.Min(result.Time, (int) (problem.TimeLimit * timeFactor));
                    result.Memory = Math.Min(result.Memory, problem.MemoryLimit);
                    return result;
                }
            }

            return new Result
            {
                Verdict = Verdict.Failed,
                FailedOn = null, Score = 0,
                Message = "Judge timeout."
            };
        }
    }
}