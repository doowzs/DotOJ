using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IProblemService
    {
        public Task<ProblemViewDto> GetProblemViewAsync(int id, bool privileged);
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, bool privileged);
        public Task<Problem> CreateProblemAsync(ProblemEditDto dto);
        public Task UpdateProblemAsync(ProblemEditDto dto);
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
            _logger = _logger;
        }
        
        public async Task ValidateProblemEditDto(ProblemEditDto dto)
        {
            if (!await _context.Assignments.AnyAsync(a => a.Id == dto.AssignmentId))
            {
                throw new ValidationException("Invalid assignment ID.");
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

        public async Task<ProblemViewDto> GetProblemViewAsync(int id, bool privileged)
        {
            var problem = await _context.Problems.FindAsync(id);
            if (!(privileged || DateTime.Now >= problem.CanBeViewedAfter))
            {
                throw new UnauthorizedAccessException("Not authorized to view this problem.");
            }
            return new ProblemViewDto(problem);
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, bool privileged)
        {
            IQueryable<Problem> data = _context.Problems;
            if (!privileged)
            {
                var now = DateTime.Now;
                data = data.Where(p => now >= p.CanBeListedAfter);
            }
            return await data.PaginateAsync(p => new ProblemInfoDto(p), pageIndex ?? 1, PageSize);
        }

        public async Task<Problem> CreateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemEditDto(dto);
            var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
            var problem = new Problem()
            {
                Id = 0,
                AssignmentId = dto.AssignmentId.GetValueOrDefault(),
                Name = dto.Name,
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
                SampleCasesSerialized = dto.SampleCases,
                TestCasesSerialized = dto.TestCases,
                AcceptedSubmissions = 0,
                TotalSubmissions = 0,
                CanBeViewedAfter = assignment.BeginTime,
                CanBeListedAfter = assignment.EndTime,
            };
            await _context.Problems.AddAsync(problem);
            await _context.SaveChangesAsync();
            return problem;
        }

        public async Task UpdateProblemAsync(ProblemEditDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteProblemAsync(int id)
        {
            var problem = new Problem() {Id = id};
            _context.Problems.Attach(problem);
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        }
    }
}
