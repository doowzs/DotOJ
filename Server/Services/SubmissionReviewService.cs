using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.RabbitMQ;
using Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Exceptions;
using Server.Services.Singleton;

namespace Server.Services
{
    public interface ISubmissionReviewService
    {
        public Task<List<SubmissionInfoDto>> GetSubmissionsReviewAsync(int problemId);
       
    }

    public class SubmissionReviewService : LoggableService<SubmissionReviewService>, ISubmissionReviewService
    {
        private readonly ProblemStatisticsService _statisticsService;
        public SubmissionReviewService(IServiceProvider provider) : base(provider)
        {
            _statisticsService = provider.GetRequiredService<ProblemStatisticsService>();
        }
        
        private async Task<Boolean> IsSubmissionReviewViewableAsync(Submission submission)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (submission.UserId == user.Id)   
            {
                return false; // Cannot review your code
            }

            if (Config.Value.ExamId.HasValue)
            {
                return false; // Cannot review code when exam
            }

            var problem = await Context.Problems.FindAsync(submission.ProblemId);
            if (problem.Type != ProblemType.Ordinary)
            {
                return false; // Cannot view lab submissions
            }

            var contest = await Context.Contests.FindAsync(problem.ContestId);
            if (DateTime.Now.ToUniversalTime() > contest.EndTime)
            {
                return false; // Cannot review code after the contest ends
            }
            
            var registerRole = await Context.Registrations
                .FirstAsync(r => r.ContestId == contest.Id && r.UserId == submission.UserId);

            if (registerRole.IsContestManager)
            {
                return false; // Cannot review ContestManager's code
            }
            
            return true;
        }

        private async Task<List<SubmissionInfoDto>> GetLegalSubmissionsReviewAsync(int problemId)
        {
            
            var submissions = await Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && s.Verdict == Verdict.Accepted)
                .Include(s => s.User)
                .OrderBy(s => s.Id)
                .ToListAsync();

            var legalSubbmissons = new List<SubmissionInfoDto>();
            var dict = new Dictionary<string, int>();
            var dictSorted = new Dictionary<SubmissionInfoDto, int>();
            
            foreach (var submission in submissions)
            {
                if (!dict.ContainsKey(submission.User.ContestantId))
                {
                    if (await IsSubmissionReviewViewableAsync(submission))
                    {
                        var count = await Context.SubmissionReviews
                            .Where(s => s.SubmissionId == submission.Id)
                            .CountAsync();

                        dict.Add(submission.User.ContestantId, count);
                        var submissionDto = new SubmissionInfoDto(submission, true);
                        dictSorted.Add(submissionDto, count);
                        legalSubbmissons.Add(submissionDto);
                    }
                }
            }

            dictSorted = dictSorted.OrderBy(f => f.Key).ToDictionary(f => f.Key, f => f.Value);

            var cnt = 0;
            legalSubbmissons = new List<SubmissionInfoDto>();
            
            foreach (var item in dictSorted)
            {
                legalSubbmissons.Add(item.Key);
                cnt = cnt + 1;
                if (cnt == 5)
                {
                    break;
                }
            }
            return legalSubbmissons;
        }

        public async Task<List<SubmissionInfoDto>> GetSubmissionsReviewAsync(int problemId)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);

            var count = await Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && s.UserId == user.Id
                            && s.Verdict == Verdict.Accepted)
                .CountAsync();
            if (count == 0)
            {
                throw new ValidationException("Cannot review before pass the problem.");
            }
            
            var submissions = await GetLegalSubmissionsReviewAsync(problemId);
            
            if (submissions.Count < 5)
            {
                throw new ValidationException($"Waiting : {submissions.Count}/5.");
            }

            return submissions;
        }
    }
}