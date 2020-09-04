using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Configs
{
    [NotMapped]
    public class ApplicationConfig
    {
        public string Host { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string MessageOfTheDay { get; set; }
        public string DataPath { get; set; }
    }

    [NotMapped]
    public class ApplicationConfigDto
    {
        public string Title { get; }
        public string Author { get; }
        public string MessageOfTheDay { get; }

        public ApplicationConfigDto(ApplicationConfig config)
        {
            Title = config.Title;
            Author = config.Author;
            MessageOfTheDay = config.MessageOfTheDay;
        }
    }
}