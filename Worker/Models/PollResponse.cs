using Data.Models;
using Newtonsoft.Json;

namespace Worker.Models
{
    public class PollResponse
    {
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("stdout")] public string Stdout { get; set; }
        [JsonProperty("stderr")] public string Stderr { get; set; }
        [JsonProperty("compile_output")] public string CompileOutput { get; set; }
        [JsonProperty("time")] public string Time { get; set; }
        [JsonProperty("wall_time")] public string WallTime { get; set; }
        [JsonProperty("memory")] public float? Memory { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("status_id")] public Verdict Verdict { get; set; }
    }
}