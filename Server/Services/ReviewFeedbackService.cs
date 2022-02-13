using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Models;
using Shared.Generics;
using Microsoft.EntityFrameworkCore;

namespace Server.Services
{
    public interface IReviewFeedBackService
    {
        public Task<List<SubmissionReviewInfoDto>> GetReviewFeedBackListAsync(int problemId);
    }

    public class ReviewFeedBackService : LoggableService<ReviewFeedBackService>, IReviewFeedBackService
    {
        public ReviewFeedBackService(IServiceProvider provider) : base(provider)
        {
            
        }

        public async Task<List<SubmissionReviewInfoDto>> GetReviewFeedBackListAsync(int problemId)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (Accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var reviews = await Context.SubmissionReviews
                    .Include(s => s.Submission)
                    .ToListAsync();
                reviews =  reviews.FindAll(s => s.Submission.ProblemId == problemId 
                                                  && s.Submission.UserId == user.Id);
                var reviewInfoDtoList = new List<SubmissionReviewInfoDto>();
                foreach (var review in reviews)
                {
                    reviewInfoDtoList.Add(new SubmissionReviewInfoDto(review.Score, review.TimeComplexity, review.SpaceComplexity, review.CodeSpecification, review.Comments,
                        new SubmissionViewDto(review.Submission, Config), user.ContestantId));
                }
                return reviewInfoDtoList;
            }
            else
            {
                return new List<SubmissionReviewInfoDto>();
            }
        }
    }
}