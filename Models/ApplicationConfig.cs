using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    [NotMapped]
    public class ApplicationConfig
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string MessageOfTheDay { get; set; }
    }

    [NotMapped]
    public class ApplicationConfigDto
    {
        public string Name { get; }
        public string Author { get; }
        public string MessageOfTheDay { get; }

        public ApplicationConfigDto(ApplicationConfig config)
        {
            Name = config.Name;
            Author = config.Author;
            MessageOfTheDay = config.MessageOfTheDay;
        }
    }
}