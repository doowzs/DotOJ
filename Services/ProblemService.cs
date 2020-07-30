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
        public Task ValidateProblemId(int id);
        public Task ValidateProblemEditDto(ProblemEditDto dto);
        public Task<ProblemViewDto> GetProblemViewAsync(int id, bool privileged);
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, bool privileged);
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
            if (!(privileged || DateTime.Now >= problem.Assignment.BeginTime))
            {
                throw new UnauthorizedAccessException("Not authorized to view this problem.");
            }
            await _context.Entry(problem).Collection(p => p.Submissions).LoadAsync();
            return new ProblemViewDto(problem);
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex, bool privileged)
        {
            IQueryable<Problem> data = _context.Problems;
            if (!privileged)
            {
                data = data.Where(p => DateTime.Now >= p.Assignment.BeginTime);
            }
            return await data.Include(p => p.Submissions)
                .PaginateAsync(p => new ProblemInfoDto(p), pageIndex ?? 1, PageSize);
        }

        public async Task<ProblemEditDto> CreateProblemAsync(ProblemEditDto dto)
        {
            await ValidateProblemEditDto(dto);
            var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
            var problem = new Problem()
            {
                Id = 0,
                AssignmentId = dto.AssignmentId.GetValueOrDefault(),
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
                SampleCasesSerialized = dto.SampleCases,
                TestCasesSerialized = dto.TestCases,
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
                AssignmentId = dto.AssignmentId.GetValueOrDefault(),
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
                SampleCasesSerialized = dto.SampleCases,
                TestCasesSerialized = dto.TestCases,
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
