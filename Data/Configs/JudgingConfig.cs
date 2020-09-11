using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Configs
{
    [NotMapped]
    public class JudgingConfig
    {
        public string Name { get; set; }
        public string DataPath { get; set; }
    }
}