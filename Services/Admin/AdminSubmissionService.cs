using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminSubmissionService
    {
        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex);

        public Task<SubmissionEditDto> GetSubmissionEditAsync(int id);
        public Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto);
        public Task<SubmissionEditDto> DeleteSubmissionAsync(int id);
    }

    public class AdminSubmissionService : IAdminSubmissionService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminSubmissionService> _logger;

        public AdminSubmissionService(ApplicationDbContext context, ILogger<AdminSubmissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<PaginatedList<SubmissionInfoDto>> GetPaginatedSubmissionInfosAsync
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex)
        {
            throw new System.NotImplementedException();
        }

        public Task<SubmissionEditDto> GetSubmissionEditAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<SubmissionEditDto> UpdateSubmissionAsync(int id, SubmissionEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<SubmissionEditDto> DeleteSubmissionAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}