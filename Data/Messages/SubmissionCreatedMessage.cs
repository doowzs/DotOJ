using System;
using Data.Models;

namespace Data.Messages
{
    public class SubmissionCreatedMessage
    {
        public int Id { get; set; }
        public int ProblemId { get; set; }
        public Program Program { get; set; }

        public SubmissionCreatedMessage(Submission submission)
        {
            Id = submission.Id;
            ProblemId = submission.ProblemId;
            Program = submission.Program;
        }
    }
}
