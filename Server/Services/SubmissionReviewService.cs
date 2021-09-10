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
        public Task<List<SubmissionInfoDto>> GetSubmissionsToReviewListAsync(int problemId);
       
    }

    public class SubmissionReviewService : LoggableService<SubmissionReviewService>, ISubmissionReviewService
    {
        public SubmissionReviewService(IServiceProvider provider) : base(provider)
        {
            
        }
        
        private async Task<Boolean> CanSubmissionReviewAsync(Submission submission)
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
                .FirstOrDefaultAsync(r => r.ContestId == contest.Id && r.UserId == submission.UserId);

            if (registerRole == null || registerRole.IsContestManager)
            {
                return false; // Cannot review ContestManager's code
            }
            
            return true;
        }

        private async Task<List<SubmissionInfoDto>> GetLegalSubmissionsToReviewListAsync(int problemId)
        {
            
            var submissions = await Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && s.Verdict == Verdict.Accepted)
                .Include(s => s.User)
                .OrderBy(s => s.Id)
                .ToListAsync();
            
            var contestantIdDict = new Dictionary<string, int>();
            var submissionInfoDtoDict = new Dictionary<SubmissionInfoDto, int>();
            
            foreach (var submission in submissions)
            {
                if (!contestantIdDict.ContainsKey(submission.User.ContestantId))
                {
                    if (await CanSubmissionReviewAsync(submission))
                    {
                        var count = await Context.SubmissionReviews
                            .Where(s => s.SubmissionId == submission.Id)
                            .CountAsync();

                        contestantIdDict.Add(submission.User.ContestantId, count);
                        var submissionDto = new SubmissionInfoDto(submission, true);
                        submissionInfoDtoDict.Add(submissionDto, count);
                    }
                }
            }

            var legalSubmissions = submissionInfoDtoDict
                .OrderBy(p => p.Value)
                .Take(5)
                .Select(p => p.Key)
                .ToList();
            return legalSubmissions;
        }

        public async Task<List<SubmissionInfoDto>> GetSubmissionsToReviewListAsync(int problemId)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            
            if (! await  Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && s.UserId == user.Id
                            && s.Verdict == Verdict.Accepted)
                .AnyAsync())
            {
                throw new ValidationException("Cannot review before pass the problem.");
            }
            
            var submissions = await GetLegalSubmissionsToReviewListAsync(problemId);
            
            if (submissions.Count < 5)
            {
                throw new ValidationException($"Waiting : {submissions.Count}/5.");
            }

            return submissions;
        }
    }
}