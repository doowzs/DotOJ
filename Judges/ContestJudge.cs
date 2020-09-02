using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Judges.Submission;
using Judge1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Judges
{
    public interface IContestJudge
    {
        public Task JudgeSubmission(int submissionId);
    }

    public class ContestJudge : IContestJudge
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IServiceProvider _provider;
        private readonly ILogger<ContestJudge> _logger;

        public ContestJudge(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IServiceProvider provider, ILogger<ContestJudge> logger)
        {
            _context = context;
            _manager = manager;
            _provider = provider;
            _logger = logger;
        }

        private async Task EnsureUserCanSubmit(ApplicationUser user, Contest contest)
        {
            var begun = DateTime.Now.ToUniversalTime() > contest.BeginTime;
            var ended = DateTime.Now.ToUniversalTime() > contest.EndTime;
            var privileged = await _manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                             await _manager.IsInRoleAsync(user, ApplicationRoles.ContestManager);

            // Only administrators can submit before contest begins.
            if (!begun && !privileged)
            {
                throw new UnauthorizedAccessException("Non-privileged submission before contest begins.");
            }

            // Any user can submit to any contest after contest ends.
            if (ended)
            {
                return;
            }

            // Otherwise, only registered user can submit to a running contest.
            bool registered = await _context.Registrations.FindAsync(user.Id, contest.Id) != null;
            if (contest.IsPublic)
            {
                if (!registered)
                {
                    await _context.Registrations.AddAsync(new Registration
                    {
                        UserId = user.Id,
                        ContestId = contest.Id,
                        IsParticipant = true,
                        IsContestManager = false,
                        Statistics = new List<ProblemStatistics>()
                    });
                }
            }
            else
            {
                if (!registered)
                {
                    throw new UnauthorizedAccessException("Not registered to a private running contest.");
                }
            }
        }

        public async Task JudgeSubmission(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                throw new ValidationException("Invalid submission ID.");
            }

            try
            {
                var user = await _manager.FindByIdAsync(submission.UserId);
                var problem = await _context.Problems.FindAsync(submission.ProblemId);
                var contest = await _context.Contests.FindAsync(problem.ContestId);
                await EnsureUserCanSubmit(user, contest);

                submission.Verdict = Verdict.Running;
                submission.FailedOn = -1;
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();

                ISubmissionJudge judge;
                switch (contest.Mode)
                {
                    case ContestMode.Practice:
                        judge = new PracticeModeJudge(_provider);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                #region Update judge result of submission

                var result = await judge.Judge(submission, problem);
                submission.Verdict = result.Verdict;
                submission.Time = result.Time;
                submission.Memory = result.Memory;
                submission.FailedOn = result.FailedOn;
                submission.Score = result.Score;
                submission.Message = result.Message;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();

                #endregion

                #region Rebuild statistics of registration

                var registration = await _context.Registrations.FindAsync(user.Id, contest.Id);
                {
                    var statistics = new List<ProblemStatistics>();

                    var problemIds = await _context.Problems
                        .Where(p => p.ContestId == contest.Id)
                        .Select(p => p.Id)
                        .ToListAsync();
                    foreach (var problemId in problemIds)
                    {
                        DateTime? acceptedAt = null;
                        int penalties = 0, score = 0;

                        var firstSolved = await _context.Submissions
                            .OrderBy(s => s.Id)
                            .Where(s => s.ProblemId == problemId && s.Verdict == Verdict.Accepted)
                            .FirstOrDefaultAsync();
                        if (firstSolved != null)
                        {
                            acceptedAt = firstSolved.CreatedAt;
                            penalties = await _context.Submissions
                                .Where(s => s.ProblemId == problemId && s.Verdict > Verdict.Accepted &&
                                            s.Id < firstSolved.Id && s.FailedOn > 0)
                                .CountAsync();
                        }
                        else
                        {
                            penalties = await _context.Submissions
                                .Where(s => s.ProblemId == problemId && s.Verdict > Verdict.Accepted && s.FailedOn > 0)
                                .CountAsync();
                        }

                        score = await _context.Submissions
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

                    registration.Statistics = statistics;
                }
                _context.Registrations.Update(registration);
                await _context.SaveChangesAsync();

                #endregion
            }
            catch (Exception e)
            {
                _logger.LogError($"Error when judging submission #{submissionId}: {e.Message}");
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = -1;
                submission.Score = 0;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                _context.Submissions.Update(submission);
                await _context.SaveChangesAsync();
            }
        }
    }
}