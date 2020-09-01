using Judge1.Data;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminSubmissionService
    {
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
    }
}