using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Data.Models;
using Worker.Models;

namespace Worker.Runners.LanguageTypes.Base
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
                var file = Path.Combine(Options.Value.DataPath, Problem.Id.ToString(), testCase.Output);
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
            var file = Path.Combine(Box, "answer");
            await using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            await using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(answer);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "isolate",
                    Arguments = $"--cg -b {BoxId} -s -E PATH=/usr/bin/ -i /dev/null -r checker_message" +
                                $" -p1 -f 409600 --cg-timing -t {TimeLimit} -x 0 -w {TimeLimit + 3.0f} -k 128000 --cg-mem={MemoryLimit}" +
                                " --run -- checker ./jail/input ./jail/output answer"
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                return true;
            }
            else if (process.ExitCode == 1)
            {
                return false;
            }
            else
            {
                throw new Exception($"SPJ isolate error ExitCode={process.ExitCode}.");
            }
        }
    }
}