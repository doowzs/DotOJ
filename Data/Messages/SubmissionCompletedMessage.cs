using System;
using Data.Models;

namespace Data.Messages
{
    class SubmissionCompletedMessage
    {
        public int Id { get; set; }
        public Verdict Verdict { get; set; }
        public string JudgedBy { get; set; }
    }
}
