using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Generics;

namespace Data.Models
{
    public class Bulletin : ModelWithTimestamps
    {
        public int Id { get; set; }
        public int Weight { get; set; }
        [Required, Column(TypeName = "text")] public string Content { get; set; }
        public DateTime? PublishAt { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}