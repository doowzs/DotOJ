namespace Data.RabbitMQ
{
    public class JudgeRequestMessage
    {
        public int SubmissionId { get; set; }
        public int RequestVersion { get; set; }
    }

    public class JudgeCompleteMessage
    {
        public int SubmissionId { get; set; }
        public int CompleteVersion { get; set; }
    }

    public class WorkerHeartbeatMessage
    {
        public string Name { get; set; }
        public string Token { get; set; }
    }
}