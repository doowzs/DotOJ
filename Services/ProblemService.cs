using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public class ProblemService
    {
        private const int PageSize = 50;
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProblemService> _logger;
        
        public ProblemService(ApplicationDbContext context, ILogger<ProblemService> logger)
        {
            _context = context;
            _logger = _logger;
        }

        public async Task<Problem> GetProblemAsync(int id)
        {
            return await _context.Problems.SingleAsync(p => p.Id == id);
        }

        public async Task<PaginatedList<Problem>> GetPaginatedProblems(int pageIndex)
        {
            return await _context.Problems.PaginateAsync(pageIndex, PageSize);
        }

        public async Task CreateProblemAsync(Problem problem)
        {
            _context.Problems.Add(problem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProblemAsync(Problem problem)
        {
            _context.Problems.Update(problem);
            await _context.SaveChangesAsync();
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
