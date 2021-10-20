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
        public Task<List<SubmissionViewDto>> GetSubmissionsToReviewListAsync(int problemId);
        public Task<string> CreateSubmissionReviewAsync(SubmissionReviewCreateDto reviewDto);
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
                return true; // Can review code after the contest ends
            }

            if (await Context.SubmissionReviews
                .Where(s => s.SubmissionId == submission.Id && s.UserId == user.Id)
                .AnyAsync())
            {
                return false;
            }
                /*if (registerRole == null || registerRole.IsContestManager)
                {
                    return false; // Cannot review ContestManager's code
                }*/
            
            return true;
        }

        private async Task<List<SubmissionViewDto>> GetLegalSubmissionsToReviewListAsync(int problemId)
        {
            
            var submissions = await Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && (s.Verdict == Verdict.Accepted || s.Verdict == Verdict.Voided))
                .Include(s => s.User)
                .OrderBy(s => s.Id)
                .ToListAsync();
            
            var contestantIdDict = new Dictionary<string, int>();
            var submissionViewDtoDict = new Dictionary<SubmissionViewDto, int>();
            
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
                        var submissionDto = new SubmissionViewDto(submission, Config);
                        submissionViewDtoDict.Add(submissionDto, count);
                    }
                }
            }

            var legalSubmissions = submissionViewDtoDict
                .OrderBy(p => p.Value)
                .Take(3)
                .Select(p => p.Key)
                .ToList();
            return legalSubmissions;
        }

        public async Task<List<SubmissionViewDto>> GetSubmissionsToReviewListAsync(int problemId)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            
            if (! await  Context.Submissions
                .Where(s => s.ProblemId == problemId
                            && s.UserId == user.Id
                            && s.Verdict == Verdict.Accepted)
                .AnyAsync())
            {
                throw new ValidationException("请先通过此题");
            }
            
            var submissions = await GetLegalSubmissionsToReviewListAsync(problemId);
            
            if (submissions.Count < 3)
            {
                throw new ValidationException($"Waiting : {submissions.Count}/3.");
            }

            return submissions;
        }

        public async Task<string> CreateSubmissionReviewAsync(SubmissionReviewCreateDto reviewDto)
        {
            var message = "提交成功";
            
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (! await  Context.Submissions
                .Where(s => s.ProblemId == reviewDto.ProblemId
                            && s.UserId == user.Id
                            && s.Verdict == Verdict.Accepted)
                .AnyAsync())
            {
                throw new ValidationException("请先通过此题");
            }

            if (reviewDto.SubmissionId.Count == 0)
            {
                throw new ValidationException("请提供有效提交");
            }

            for (var i = 0; i < reviewDto.SubmissionId.Count; i++)
            {
                var submissionId = reviewDto.SubmissionId[i];
                var score = reviewDto.Score[i];
                var comment = reviewDto.Comments[i];
                if (await Context.SubmissionReviews
                    .Include(s => s.Submission)
                    .Where(s => s.SubmissionId == submissionId
                                && s.UserId == user.Id)
                    .AnyAsync())
                {
                    throw new ValidationException("请不要重复提交");
                }

                var review = new SubmissionReview
                {
                    UserId = user.Id,
                    SubmissionId = submissionId,
                    Score = score,
                    Comments = comment
                };
                await Context.SubmissionReviews.AddAsync(review);
            }
           
            await Context.SaveChangesAsync();
            return message;
        }
    }
}