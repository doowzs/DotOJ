using Data.Models;

namespace Worker.Models
{
    public class Result
    {
        public Verdict Verdict { get; set; }
        public int Time { get; set; }
        public int Memory { set; get; }
        public string Message { set; get; }
        public int? FailedOn { get; set; }
        public int Score { get; set; }
    }
}
