using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shared.Archives.v2.TestKit
{
    public class Config
    {
        public int? Time { get; set; }
        public int? Memory { get; set; }
        public int? Thread { get; set; }
        public int? Disk { get; set; }
        public string Input { get; set; }
        public Dictionary<string, string> Files { get; set; }
        [JsonRequired] public string Command { get; set; }
    }

    public class Step
    {
        [JsonRequired] public string Title { get; set; }
        public bool Hidden { get; set; }
        public bool Bail { get; set; }
        public int Score { get; set; }
        public List<string> Groups { get; set; }
        [JsonRequired] public Config Execute { get; set; }
        public Config Validate { get; set; }
    }

    public class Stage
    {
        [JsonRequired] public string Title { get; set; }
        public bool Hidden { get; set; }
        public bool Bail { get; set; }
        public List<string> Groups { get; set; }
        [JsonRequired] public List<Step> Steps { get; set; }
    }

    public class Manifest
    {
        [JsonRequired] public string Version { get; set; }
        public Config Default { get; set; }
        public Dictionary<string, List<string>> Groups { get; set; }
        [JsonRequired] public List<Stage> Stages { get; set; }
    }
}