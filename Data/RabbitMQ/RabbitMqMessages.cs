namespace Data.RabbitMQ
{
    public enum JobType
    {
        JudgeSubmission = 1,
        CheckPlagiarism = 2
    }

    public class JobRequestMessage
    {
        public JobType JobType { get; set; }
        public int TargetId { get; set; }
        public int RequestVersion { get; set; }

        public JobRequestMessage(JobType jobType, int targetId, int requestVersion)
        {
            JobType = jobType;
            TargetId = targetId;
            RequestVersion = requestVersion;
        }
    }

    public class JobCompleteMessage
    {
        public JobType JobType { get; set; }
        public int TargetId { get; set; }
        public int CompleteVersion { get; set; }

        public JobCompleteMessage(JobType jobType, int targetId, int completeVersion)
        {
            JobType = jobType;
            TargetId = targetId;
            CompleteVersion = completeVersion;
        }
    }

    public class WorkerHeartbeatMessage
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }
}