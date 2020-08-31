using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminUserService
    {
        public Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex);
        public Task<ApplicationUserEditDto> GetUserEditAsync(string id);
    }

    public class AdminUserService : IAdminUserService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            ILogger<AdminUserService> logger)
        {
            _context = context;
            _manager = manager;
            _logger = logger;
        }

        private async Task EnsureUserExists(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException();
            }
        }

        public async Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex)
        {
            return await _context.Users.PaginateAsync(u => new ApplicationUserInfoDto(u), pageIndex ?? 1, PageSize);
        }

        public async Task<ApplicationUserEditDto> GetUserEditAsync(string id)
        {
            await EnsureUserExists(id);
            var user = await _context.Users.FindAsync(id);
            var roles = await _manager.GetRolesAsync(user);
            return new ApplicationUserEditDto(user, roles);
        }
    }
}