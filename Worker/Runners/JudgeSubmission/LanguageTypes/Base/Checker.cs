using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.Base
{
    public abstract partial class Runner
    {
        private async Task<bool> CheckTestCaseOutputAsync(Run run, bool inline, TestCase testCase)
        {
            if (run.Verdict > Verdict.Accepted)
            {
                return true; // do not change a failed state
            }

            string answer = null;
            if (inline)
            {
                answer = Encoding.UTF8.GetString(Convert.FromBase64String(testCase.Output)).TrimEnd();
            }
            else
            {
                var file = Path.Combine(_options.Value.DataPath, "tests", Problem.Id.ToString(), testCase.Output);
                await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                answer = (await reader.ReadToEndAsync()).TrimEnd();
            }

            if (!Problem.HasSpecialJudge)
            {
                return await CheckTestCaseOutputPlainAsync(run.Stdout, answer);
            }
            else
            {
                return await CheckTestCaseOutputSpecialJudgeAsync(answer);
            }
        }

        private async Task<bool> CheckTestCaseOutputPlainAsync(string output, string answer)
        {
            using var outputReader = new StringReader(output);
            using var answerReader = new StringReader(answer);
            var outputLine = await outputReader.ReadLineAsync();
            var answerLine = await answerReader.ReadLineAsync();
            while (outputLine != null && answerLine != null)
            {
                outputLine = outputLine.TrimEnd();
                answerLine = answerLine.TrimEnd();
                if (!outputLine.Equals(answerLine))
                {
                    return false;
                }

                outputLine = await outputReader.ReadLineAsync();
                answerLine = await answerReader.ReadLineAsync();
            }

            while (outputLine != null)
            {
                if (!string.IsNullOrWhiteSpace(outputLine))
                {
                    return false;
                }

                outputLine = await outputReader.ReadLineAsync();
            }

            while (answerLine != null)
            {
                if (!string.IsNullOrWhiteSpace(answerLine))
                {
                    return false;
                }

                answerLine = await answerReader.ReadLineAsync();
            }

            return true;
        }


        private async Task<bool> CheckTestCaseOutputSpecialJudgeAsync(string answer)
        {
            var file = Path.Combine(Root, "answer");
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(answer);
            }
            return await Box.ExecuteAsync(
                "checker jail/input jail/output answer",
                stderr: "checker_message",
                time: TimeLimit,
                memory: MemoryLimit
            ) == 0;
        }
    }
}