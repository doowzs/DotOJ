using Newtonsoft.Json;

namespace Worker.Models
{
    public class TokenResponse
    {
        [JsonProperty("token")] public string Token { get; set; }
    }
}