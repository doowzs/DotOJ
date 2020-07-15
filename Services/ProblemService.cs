using System;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.AspNetCore.Authorization;
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
            throw new NotImplementedException();
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
