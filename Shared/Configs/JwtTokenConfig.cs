using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Configs
{
    [NotMapped]
    public class JwtTokenConfig
    {
        public string Issuer { get; set; }
        public string Secret { get; set; }
        public string Audience { get; set; }
        public int Expires { get; set; } // minutes
    }
}