using Data.Models;

namespace Worker.Models
{
    public class Run
    {
        public int Index { get; set; }
        public string Token { get; set; }

        public Verdict Verdict { get; set; }
        public float? Time { get; set; }
        public float? WallTime { get; set; }
        public float? Memory { set; get; }
        public string Message { set; get; }
    }
}
