using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Data.Models
{
    [NotMapped]
    public class ProblemStatistics
    {
        public int ProblemId { get; set; }
        public int Penalties { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public int Score { get; set; }
    }

    public class Registration : ModelWithTimestamps
    {
        // Table uses composite key: (UserID, ContestId).
        [Required] public string UserId { get; set; }
        public int ContestId { get; set; }
        public ApplicationUser User { get; set; }
        public Contest Contest { get; set; }

        public bool IsParticipant { get; set; }
        public bool IsContestManager { get; set; }

        #region Statistics

        [NotMapped] public List<ProblemStatistics> Statistics;

        [Column("statistics", TypeName = "text")]
        public string StatisticsSerialized
        {
            get => JsonConvert.SerializeObject(Statistics);
            set => Statistics = string.IsNullOrEmpty(value)
                ? new List<ProblemStatistics>()
                : JsonConvert.DeserializeObject<List<ProblemStatistics>>(value);
        }

        #endregion

        public async Task RebuildStatisticsAsync(ApplicationDbContext context)
        {
            var statistics = new List<ProblemStatistics>();

            var contest = await context.Contests.FindAsync(ContestId);
            var problemIds = await context.Problems
                .Where(p => p.ContestId == ContestId)
                .Select(p => p.Id)
                .ToListAsync();
            var userSubmissions = context.Submissions
                .Where(s => s.UserId == UserId && s.CreatedAt >= contest.BeginTime && s.CreatedAt <= contest.EndTime);
            foreach (var problemId in problemIds)
            {
                if (!await userSubmissions.AnyAsync(s => s.ProblemId == problemId))
                {
                    continue;
                }

                DateTime? acceptedAt = null;
                int penalties = 0, score = 0;
                var problemSubmissions = userSubmissions.Where(s => s.ProblemId == problemId);
                var firstSolved = await problemSubmissions
                    .OrderBy(s => s.Id)
                    .Where(s => s.Verdict == Verdict.Accepted)
                    .FirstOrDefaultAsync();
                if (firstSolved != null)
                {
                    acceptedAt = firstSolved.CreatedAt;
                    penalties = await problemSubmissions
                        .Where(s => s.Verdict > Verdict.Accepted && s.Id < firstSolved.Id && s.FailedOn > 0)
                        .CountAsync();
                }
                else
                {
                    penalties = await problemSubmissions
                        .Where(s => s.ProblemId == problemId && s.Verdict > Verdict.Accepted && s.FailedOn > 0)
                        .CountAsync();
                }

                score = await problemSubmissions
                    .Where(s => s.ProblemId == problemId && s.Score.HasValue)
                    .MaxAsync(s => s.Score.GetValueOrDefault());

                var problemStatistics = new ProblemStatistics
                {
                    ProblemId = problemId,
                    AcceptedAt = acceptedAt,
                    Penalties = penalties,
                    Score = score
                };
                statistics.Add(problemStatistics);
            }

            Statistics = statistics;
        }
    }

    [NotMapped]
    public class RegistrationInfoDto : DtoWithTimestamps
    {
        public string UserId { get; }
        public string ContestantId { get; }
        public string ContestantName { get; }
        public int ContestId { get; }
        public bool IsParticipant { get; }
        public bool IsContestManager { get; }
        public List<ProblemStatistics> Statistics { get; }

        public RegistrationInfoDto(Registration registration) : base(registration)
        {
            UserId = registration.UserId;
            ContestId = registration.ContestId;
            ContestantId = registration.User.ContestantId;
            ContestantName = registration.User.ContestantName;
            IsParticipant = registration.IsParticipant;
            IsContestManager = registration.IsContestManager;
            Statistics = registration.Statistics;
        }
    }
}