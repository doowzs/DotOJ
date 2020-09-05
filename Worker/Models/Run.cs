using Data.Models;

namespace Worker.Models
{
    public class Run
    {
        public int Index { get; set; }
        public int TimeLimit { get; set; }
        public string Token { get; set; }

        public Verdict Verdict { get; set; }
        public int? Time { get; set; }
        public int? Memory { set; get; }
        public string Message { set; get; }
    }
}
