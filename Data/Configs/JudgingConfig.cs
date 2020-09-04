using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Configs
{
    [NotMapped]
    public class JudgeInstance
    {
        public string Endpoint { get; set; }
        public string AuthUser { get; set; }
        public string AuthToken { get; set; }
    }

    [NotMapped]
    public class JudgingConfig
    {
        public string Name { get; set; }
        public string DataPath { get; set; }
        public JudgeInstance Instance { get; set; }
    }
}