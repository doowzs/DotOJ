using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Configs
{
    [NotMapped]
    public class ApplicationConfig
    {
        public string Host { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string DataPath { get; set; }
        public int? ExamId { get; set; }
        public readonly string Version = "latest"; // injected in Dockerfile.webapp
    }

    [NotMapped]
    public class ApplicationConfigDto
    {
        public string Title { get; }
        public string Author { get; }
        public int? ExamId { get; }
        public string Version { get; }
        public DateTime ServerTime { get; }

        public ApplicationConfigDto(ApplicationConfig config)
        {
            Title = config.Title;
            Author = config.Author;
            ExamId = config.ExamId;
            Version = config.Version;
            ServerTime = DateTime.Now.ToUniversalTime();
        }
    }
}