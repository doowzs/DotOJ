using System.Collections.Generic;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private async IAsyncEnumerable<JudgeResult> RunStagesAsync()
        {
            foreach (var stage in _manifest.Stages)
            {
                if (stage.Title.ToLower().Equals("compile") && stage.Steps != null)
                {
                    stage.Bail = true;
                    foreach (var step in stage.Steps)
                    {
                        step.Bail = true;
                    }
                }

                if (IsStudentInGroups(stage.Groups))
                {
                    await foreach (var result in RunStepsAsync(stage))
                    {
                        yield return result;
                        if (result.Verdict == Verdict.CompilationError ||
                            (result.Verdict != Verdict.Accepted && stage.Bail))
                        {
                            yield break;
                        }
                    }
                }
            }
        }
    }
}