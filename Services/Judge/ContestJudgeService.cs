using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Judge1.Models;
using Judge1.Services.Judge.Submission;

namespace Judge1.Services.Judge
{
    public interface IContestJudgeService
    {
        public Task JudgeSubmission(int submissionId);
    }

    public class ContestJudgeService : LoggableService<ContestJudgeService>, IContestJudgeService
    {
        public ContestJudgeService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureUserCanSubmit(ApplicationUser user, Contest contest)
        {
            var begun = DateTime.Now.ToUniversalTime() > contest.BeginTime;
            var ended = DateTime.Now.ToUniversalTime() > contest.EndTime;
            var privileged = await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                             await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager);

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
            bool registered = await Context.Registrations.FindAsync(user.Id, contest.Id) != null;
            if (contest.IsPublic)
            {
                if (!registered)
                {
                    await Context.Registrations.AddAsync(new Registration
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
            var submission = await Context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                throw new ValidationException("Invalid submission ID.");
            }

            try
            {
                await LogInformation($"JudgeSubmission Start Id={submission.Id} Problem={submission.ProblemId}");

                var user = await Manager.FindByIdAsync(submission.UserId);
                var problem = await Context.Problems.FindAsync(submission.ProblemId);
                var contest = await Context.Contests.FindAsync(problem.ContestId);
                await EnsureUserCanSubmit(user, contest);

                submission.Verdict = Verdict.Running;
                submission.FailedOn = -1;
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();

                ISubmissionJudgeService judgeService;
                switch (contest.Mode)
                {
                    case ContestMode.Practice:
                        judgeService = new PracticeModeJudgeService(Provider);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                #region Update judge result of submission

                var result = await judgeService.Judge(submission, problem);
                submission.Verdict = result.Verdict;
                submission.Time = result.Time;
                submission.Memory = result.Memory;
                submission.FailedOn = result.FailedOn;
                submission.Score = result.Score;
                submission.Message = result.Message;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();

                #endregion

                #region Rebuild statistics of registration

                var registration = await Context.Registrations.FindAsync(user.Id, contest.Id);
                await registration.RebuildStatisticsAsync(Context);
                Context.Registrations.Update(registration);
                await Context.SaveChangesAsync();

                #endregion

                await LogInformation($"JudgeSubmission Complete Id={submissionId} Problem={submission.ProblemId} " +
                                     $"Verdict={submission.Verdict} Score={submission.Score} CreatedAt={submission.CreatedAt} JudgedAt={submission.JudgedAt}");
            }
            catch (Exception e)
            {
                submission.Verdict = Verdict.Failed;
                submission.FailedOn = -1;
                submission.Score = 0;
                submission.JudgedAt = DateTime.Now.ToUniversalTime();
                Context.Submissions.Update(submission);
                await Context.SaveChangesAsync();
                await LogError($"JudgeSubmission Error Id={submissionId} Error={e.Message}");
            }
        }
    }
}