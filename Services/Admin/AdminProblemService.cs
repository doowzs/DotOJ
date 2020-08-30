using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminProblemService
    {
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex);
        public Task<ProblemEditDto> GetProblemEditAsync(int id);
        public Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto);
        public Task<ProblemEditDto> UpdateProblemAsync(int id, ProblemEditDto dto);
        public Task DeleteProblemAsync(int id);
    }

    public class AdminProblemService : IAdminProblemService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<AdminProblemService> _logger;

        public AdminProblemService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IHttpContextAccessor accessor, ILogger<AdminProblemService> logger)
        {
            _context = context;
            _manager = manager;
            _accessor = accessor;
            _logger = logger;
        }

        private async Task EnsureProblemExists(int id)
        {
            if (!await _context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateProblemEditDtoAsync(ProblemEditDto dto)
        {
            var contest = await _context.Contests.FindAsync(dto.ContestId.GetValueOrDefault());
            if (contest == null)
            {
                throw new ValidationException("Invalid contest ID.");
            }

            if (string.IsNullOrEmpty(dto.Title))
            {
                throw new ValidationException("Invalid problem title.");
            }

            if (string.IsNullOrEmpty(dto.Description))
            {
                throw new ValidationException("Invalid problem description.");
            }

            if (string.IsNullOrEmpty(dto.InputFormat) || string.IsNullOrEmpty(dto.OutputFormat))
            {
                throw new ValidationException("Invalid problem input or output format.");
            }

            if (dto.TimeLimit < 100 || dto.TimeLimit > 60000)
            {
                throw new ValidationException("Invalid problem time limit.");
            }

            if (dto.MemoryLimit < 1000 || dto.MemoryLimit > 2 * 1024 * 1024)
            {
                throw new ValidationException("Invalid problem memory limit.");
            }

            if (dto.HasHacking || dto.HasSpecialJudge)
            {
                throw new NotImplementedException("Hacking and Special Judge is not implemented.");
            }

            if (dto.SampleCases.Count == 0)
            {
                throw new ValidationException("At lease one sample case is required.");
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            return await _context.Problems.PaginateAsync(p => new ProblemInfoDto(p), pageIndex ?? 1, PageSize);
        }

        public async Task<ProblemEditDto> GetProblemEditAsync(int id)
        {
            await EnsureProblemExists(id);
            return new ProblemEditDto(await _context.Problems.FindAsync(id));
        }

        public async Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemEditDtoAsync(dto);
            var problem = new Problem
            {
                ContestId = dto.ContestId.GetValueOrDefault(),
                Title = dto.Title,
                Description = dto.Description,
                InputFormat = dto.InputFormat,
                OutputFormat = dto.OutputFormat,
                FootNote = dto.FootNote,
                TimeLimit = dto.TimeLimit.GetValueOrDefault(),
                MemoryLimit = dto.MemoryLimit.GetValueOrDefault(),
                HasHacking = false,
                HasSpecialJudge = false,
                SampleCases = dto.SampleCases,
                TestCases = new List<TestCase>()
            };
            await _context.Problems.AddAsync(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task<ProblemEditDto> UpdateProblemAsync(int id, ProblemEditDto dto)
        {
            await EnsureProblemExists(id);
            await ValidateProblemEditDtoAsync(dto);
            var problem = await _context.Problems.FindAsync(id);
            problem.ContestId = dto.ContestId.GetValueOrDefault();
            problem.Title = dto.Title;
            problem.Description = dto.Description;
            problem.InputFormat = dto.InputFormat;
            problem.OutputFormat = dto.OutputFormat;
            problem.FootNote = dto.FootNote;
            problem.TimeLimit = dto.TimeLimit.GetValueOrDefault();
            problem.MemoryLimit = dto.MemoryLimit.GetValueOrDefault();
            problem.HasHacking = false;
            problem.HasSpecialJudge = false;
            problem.SampleCases = dto.SampleCases;
            _context.Problems.Update(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task DeleteProblemAsync(int id)
        {
            await EnsureProblemExists(id);
            var problem = new Problem {Id = id};
            _context.Problems.Attach(problem);
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        }
    }
}