using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IProblemService
    {
        public Task ValidateProblemId(int id);
        public Task ValidateProblemEditDto(ProblemEditDto dto);
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, string userId);
        public Task<ProblemViewDto> GetProblemViewAsync(int id, string userId);
        public Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto);
        public Task<ProblemEditDto> UpdateProblemAsync(ProblemEditDto dto);
        public Task DeleteProblemAsync(int id);
    }

    public class ProblemService : IProblemService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProblemService> _logger;

        public ProblemService(ApplicationDbContext context, ILogger<ProblemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ValidateProblemId(int id)
        {
            if (!await _context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new ValidationException("Invalid problem ID.");
            }
        }

        public async Task ValidateProblemEditDto(ProblemEditDto dto)
        {
            if (!await _context.Contests.AnyAsync(a => a.Id == dto.ContestId))
            {
                throw new ValidationException("Invalid contest ID.");
            }

            if (dto.HasSpecialJudge)
            {
                if (dto.SpecialJudgeProgram is null)
                {
                    throw new ValidationException("Special judge problem cannot be null.");
                }
            }

            if (dto.HasHacking)
            {
                if (dto.StandardProgram is null)
                {
                    throw new ValidationException("Standard program cannot be null.");
                }
                else if (dto.ValidatorProgram is null)
                {
                    throw new ValidationException("Validator program cannot be null.");
                }
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, string userId)
        {
            var problems = await _context.Problems
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<ProblemInfoDto> infos;
            if (userId != null)
            {
                infos = new List<ProblemInfoDto>();
                foreach (var problem in problems.Items)
                {
                    var solved = await _context.Submissions
                        .AnyAsync(s => s.ProblemId == problem.Id && s.UserId == userId && s.FailedOn == -1);
                    infos.Add(new ProblemInfoDto(problem, solved));
                }
            }
            else
            {
                infos = problems.Items.Select(p => new ProblemInfoDto(p)).ToList();
            }
            
            return new PaginatedList<ProblemInfoDto>(problems.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ProblemViewDto> GetProblemViewAsync(int id, string userId)
        {
            var problem = await _context.Problems.FindAsync(id);
            if (problem is null)
            {
                throw new NotFoundException();
            }

            await _context.Entry(problem).Reference(p => p.Contest).LoadAsync();
            if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime)
            {
                throw new UnauthorizedAccessException("Not authorized to view this problem.");
            }

            if (userId is null)
            {
                return new ProblemViewDto(problem);
            }
            else
            {
                var submissions = await _context.Submissions
                    .Where(s => s.ProblemId == id && s.UserId == userId)
                    .ToListAsync();
                return new ProblemViewDto(problem, submissions);
            }
        }

        public async Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemEditDto(dto);
            var contest = await _context.Contests.FindAsync(dto.ContestId);
            var problem = new Problem()
            {
                Id = 0,
                ContestId = dto.ContestId.GetValueOrDefault(),
                Title = dto.Title,
                Description = dto.Description,
                InputFormat = dto.InputFormat,
                OutputFormat = dto.OutputFormat,
                FootNote = dto.FootNote,
                TimeLimit = dto.TimeLimit.GetValueOrDefault(),
                MemoryLimit = dto.MemoryLimit.GetValueOrDefault(),
                HasSpecialJudge = dto.HasSpecialJudge,
                SpecialJudgeProgramSerialized = dto.SpecialJudgeProgram,
                HasHacking = dto.HasHacking,
                StandardProgramSerialized = dto.StandardProgram,
                ValidatorProgramSerialized = dto.ValidatorProgram,
                SampleCases = dto.SampleCases,
                TestCases = dto.TestCases,
            };
            await _context.Problems.AddAsync(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task<ProblemEditDto> UpdateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemId(dto.Id);
            await ValidateProblemEditDto(dto);
            var oldProblem = await _context.Problems.FindAsync(dto.Id);
            var problem = new Problem()
            {
                Id = dto.Id,
                ContestId = dto.ContestId.GetValueOrDefault(),
                Title = dto.Title,
                Description = dto.Description,
                InputFormat = dto.InputFormat,
                OutputFormat = dto.OutputFormat,
                FootNote = dto.FootNote,
                TimeLimit = dto.TimeLimit.GetValueOrDefault(),
                MemoryLimit = dto.MemoryLimit.GetValueOrDefault(),
                HasSpecialJudge = dto.HasSpecialJudge,
                SpecialJudgeProgramSerialized = dto.SpecialJudgeProgram,
                HasHacking = dto.HasHacking,
                StandardProgramSerialized = dto.StandardProgram,
                ValidatorProgramSerialized = dto.ValidatorProgram,
                SampleCases = dto.SampleCases,
                TestCases = dto.TestCases,
            };
            _context.Problems.Update(problem);
            await _context.SaveChangesAsync();
            return new ProblemEditDto(problem);
        }

        public async Task DeleteProblemAsync(int id)
        {
            await ValidateProblemId(id);
            var problem = new Problem() {Id = id};
            _context.Problems.Attach(problem);
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        }
    }
}