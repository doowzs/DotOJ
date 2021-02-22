using Data.Generics;

namespace Data.Models
{
    public class Plagiarism : ModelWithTimestamps
    {
        public int Id { get; set; }
        public string LogFile { get; set; }
        public string Report { get; set; }
        public bool Deleted { get; set; }
        public int RequestVersion { get; set; }
        public int CompleteVersion { get; set; }
    }
}