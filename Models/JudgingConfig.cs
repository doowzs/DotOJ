using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judge1.Models
{
    [NotMapped]
    public class JudgeInstance
    {
        public string Name { get; set; }
        public string Endpoint { get; set; }
    }

    [NotMapped]
    public class JudgingConfig
    {
        public string DataPath { get; set; }
        public List<JudgeInstance> Instances { get; set; }
    }
}