using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface ISubmissionService
    {
        public Task<bool> CanViewSubmission(Submission submission, string userId);
        public Task ValidateSubmissionViewDto(SubmissionViewDto dto, string userId);
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? pageIndex);
        public Task<SubmissionViewDto> GetSubmissionViewAsync(int id, string userId);
        public Task<SubmissionViewDto> CreateSubmissionAsync(SubmissionViewDto dto, string userId);
    }
    
    public class SubmissionService : ISubmissionService
    {
        private const int PageSize = 50;
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(ApplicationDbContext context, ILogger<SubmissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CanViewSubmission(Submission submission, string userId)
        {
            return submission.UserId == userId
                   || await _context.Submissions.AnyAsync(s => s.Id == submission.Id 
                                                               && s.UserId == userId 
                                                               && s.Verdict == Verdict.Accepted);
        }

        public async Task ValidateSubmissionViewDto(SubmissionViewDto dto, string userId)
        {
            if (dto.UserId != userId)
            {
                throw new ValidationException("Invalid user ID.");
            }

            var problem = await _context.Problems.FindAsync(dto.ProblemId);
            if (problem is null)
            {
                throw new ValidationException("Invalid problem ID.");
            }

            var assignment = await _context.Assignments.FindAsync(problem.AssignmentId);
            if (assignment.IsPublic)
            {
                if (DateTime.Now < assignment.BeginTime)
                {
                    throw new UnauthorizedAccessException("Cannot submit until assignment has begun.");
                }
            }
            else
            {
                var registered = await _context.AssignmentRegistrations
                    .AnyAsync(r => r.AssignmentId == assignment.Id && r.UserId == userId);
                if (registered && DateTime.Now < assignment.BeginTime)
                {
                    throw new UnauthorizedAccessException("Cannot submit until assignment has begun.");
                }
                else if (DateTime.Now < assignment.EndTime)
                {
                    throw new UnauthorizedAccessException("Cannot submit until assignment has ended.");
                }
            }
        }

        public async Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionsAsync(int? pageIndex)
        {
            return await _context.Submissions.PaginateAsync(s => new SubmissionInfoDto(s), pageIndex ?? 1, PageSize);
        }

        public async Task<SubmissionViewDto> GetSubmissionViewAsync(int id, string userId)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                throw new NotFoundException();
            }

            if (!await CanViewSubmission(submission, userId))
            {
                throw new UnauthorizedAccessException("Not allowed to view this submission.");
            }
            
            return new SubmissionViewDto(submission);
        }

        public async Task<SubmissionViewDto> CreateSubmissionAsync(SubmissionViewDto dto, string userId)
        {
            await ValidateSubmissionViewDto(dto, userId);
            var submission = new Submission()
            {
                UserId = dto.UserId,
                ProblemId = dto.ProblemId.GetValueOrDefault(),
                Program = dto.Program
            };
            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();
            return new SubmissionViewDto(submission);
        }
    }
}