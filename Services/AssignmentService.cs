using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IAssignmentService
    {
        public Task<AssignmentViewDto> GetAssignmentViewAsync(int id);
        public Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex);
        public Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto);
        public Task<AssignmentEditDto> UpdateAssignmentAsync(AssignmentEditDto dto);
        public Task DeleteAssignmentAsync(int id);
        public Task RegisterUserForAssignmentAsync(int id, ApplicationUser user);
        public Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user);
    }

    public class AssignmentService : IAssignmentService
    {
        private const int PageSize = 20;
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(ApplicationDbContext context, ILogger<AssignmentService> logger)
        {
            context = _context;
            logger = _logger;
        }

        public async Task<AssignmentViewDto> GetAssignmentViewAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex)
        {
            throw new System.NotImplementedException();
        }

        public async Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public async Task<AssignmentEditDto> UpdateAssignmentAsync(AssignmentEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task RegisterUserForAssignmentAsync(int id, ApplicationUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}