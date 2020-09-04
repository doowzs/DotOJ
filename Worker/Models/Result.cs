using System;
using Data.Models;

namespace Worker.Models
{
    public class Result
    {
        public Verdict Verdict { get; set; }
        public int? Time { get; set; }
        public int? Memory { set; get; }
        public int? FailedOn { get; set; }
        public int Score { get; set; }
        public string Message { set; get; }
        
        public static readonly Result NoTestCaseFailure = new Result
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = "No test case available"
        };

        public static readonly Result TimeoutFailure = new Result
        {
            Verdict = Verdict.Failed,
            Time = null, Memory = null,
            FailedOn = null, Score = 0,
            Message = "Worker runner timed out."
        };
    }
}