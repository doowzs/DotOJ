using System;
using System.Collections.Generic;
using Data.Generics;
using Data.Models;

namespace Data.DTOs
{
    public class PlagiarismInfoDto : DtoWithTimestamps
    {
        public int Id { get; set; }
        public int ProblemId { get; set; }
        public List<PlagiarismResult> Results { get; set; }
        public bool Outdated { get; set; }
        public DateTime? CheckedAt { get; set; }
        public string CheckedBy { get; set; }

        public PlagiarismInfoDto(Plagiarism plagiarism) : base(plagiarism)
        {
            Id = plagiarism.Id;
            ProblemId = plagiarism.ProblemId;
            Results = plagiarism.Results;
            Outdated = plagiarism.Outdated;
            CheckedAt = plagiarism.CheckedAt;
            CheckedBy = plagiarism.CheckedBy;
        }
    }
}