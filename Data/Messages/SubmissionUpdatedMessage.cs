using System;

namespace Data.Messages
{
    class SubmissionUpdatedMessage
    {
        public int Id { get; set; }
        public int Progress { get; set; }
        public string JudgedBy { get; set; }
    }
}
