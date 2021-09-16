using Shared.Models;
using System.Collections.Generic;

namespace Worker.Models
{
    public class JudgeResult
    {
        public Verdict Verdict { get; set; }
        public int? Time { get; set; }
        public int? Memory { set; get; }
        public List<int> FailedOn { get; set; }
        public int Score { get; set; }
        public string Message { set; get; }

        public static readonly JudgeResult NoTestCaseFailure = new JudgeResult
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = "No test case available."
        };

        public static readonly JudgeResult TimeoutFailure = new JudgeResult
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = "Worker runner timed out."
        };

        public static readonly JudgeResult UnknownLanguageFailure = new JudgeResult
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = "Unknown program language."
        };

        public static JudgeResult NewFailedResult(string message)
        {
            return new JudgeResult
            {
                Verdict = Verdict.Failed,
                FailedOn = new List<int>(
                    new int[]{0}
                ),
                Time = null,
                Memory = null,
                Score = 0,
                Message = message
            };
        }

        public static JudgeResult NewRejectedResult(string message)
        {
            return new JudgeResult
            {
                Verdict = Verdict.Rejected,
                FailedOn = new List<int>(
                    new int[]{0}
                    ),
                Time = null,
                Memory = null,
                Score = 0,
                Message = message
            };
        }
    }
}