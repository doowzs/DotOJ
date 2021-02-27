using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Shared.Generics;

namespace Shared.Models
{
    public class PlagiarismResult
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Path { get; set; }
    }

    public class Plagiarism : ModelWithTimestamps
    {
        public int Id { get; set; }
        public int ProblemId { get; set; }

        [NotMapped] public List<PlagiarismResult> Results { get; set; }

        [Column("Results", TypeName = "text")]
        public string ResultsSerialized
        {
            get => JsonConvert.SerializeObject(Results);
            set => Results = string.IsNullOrEmpty(value)
                ? new List<PlagiarismResult>()
                : JsonConvert.DeserializeObject<List<PlagiarismResult>>(value);
        }

        public bool Outdated { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string CheckedBy { get; set; }
        public int RequestVersion { get; set; }
        public int CompleteVersion { get; set; }
    }
}