using System.Threading.Tasks;
using Judge1.Models;

namespace Judge1.Judges.Submission
{
    public class RunInfo
    {
        public int Index { get; set; }
        public string Token { get; set; }
        public Verdict Verdict { get; set; }
    }
    
    public class JudgeResult
    {
        public Verdict Verdict { get; set; }
        public int FailedOn { get; set; }
        public int Score { get; set; }
    }

    public interface ISubmissionJudge
    {
        public Task<JudgeResult> Judge(Models.Submission submission, Problem problem);
    }
}