using System;
using System.Text;
using Data.Models;

namespace Worker.Models
{
    public class JudgeResult
    {
        public Verdict Verdict { get; set; }
        public int? Time { get; set; }
        public int? Memory { set; get; }
        public int? FailedOn { get; set; }
        public int Score { get; set; }
        public string Message { set; get; }

        public static readonly JudgeResult NoTestCaseFailure = new JudgeResult
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = Convert.ToBase64String(Encoding.UTF8.GetBytes("No test case available."))
        };

        public static readonly JudgeResult TimeoutFailure = new JudgeResult
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = Convert.ToBase64String(Encoding.UTF8.GetBytes("Worker runner timed out."))
        };
    }
}