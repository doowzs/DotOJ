using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shared.Archives.v2.TestKit;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private async IAsyncEnumerable<JudgeResult> RunStepsAsync(Stage stage)
        {
            foreach (var step in stage.Steps)
            {
                if (IsStudentInGroups(step.Groups))
                {
                    var score = 0;
                    string message = null;

                    var verdict = await RunSingleAsync(step.Execute);
                    if (verdict == Verdict.Accepted)
                    {
                        if (step.Validate == null)
                        {
                            score = step.Score;
                        }
                        else
                        {
                            verdict = await RunSingleAsync(step.Validate, "_validate");
                            verdict = verdict == Verdict.Accepted ? Verdict.Accepted : Verdict.WrongAnswer;

                            var dict = await _box.ReadDictAsync(Path.Combine(Jail, "stdout_validate"));
                            if (dict.TryGetValue("score", out var temp) && int.TryParse(temp, out score))
                            {
                                score = Math.Min(score, step.Score);
                            }
                        }
                        if (!stage.Title.ToLower().Equals("compile") && !step.Title.ToLower().Equals("compile"))
                        {
                            var output = await _box.ReadFileAsync(Path.Combine(Jail, "stderr_validate"));
                            if (output.Length > 64)
                            {
                                output = output.Substring(0, 64) + "***";
                            }
                            if (!stage.Hidden && !step.Hidden)
                            {
                                message = $"Step {stage.Title}.{step.Title}: {score}/{step.Score}\n{output}";
                            }
                        }
                    }
                    else if (stage.Title.ToLower().Equals("compile") || step.Title.ToLower().Equals("compile"))
                    {
                        var output = await _box.ReadFileAsync("compiler_output");
                        if (output.Length > 4096)
                        {
                            output = output.Substring(0, 4096)
                                     + "\n*** Output trimmed due to excessive length of 4096 characters. ***";
                        }
                        verdict = Verdict.CompilationError;
                        message = $"Step {stage.Title}.{step.Title}: Compile Error:\n{output}";
                    }

                    var failed = verdict != Verdict.Accepted;
                    yield return new JudgeResult
                    {
                        Verdict = verdict,
                        Time = null,
                        Memory = null,
                        Score = score,
                        Message = message
                    };
                    if (failed && step.Bail) yield break;
                }
            }
        }
    }
}