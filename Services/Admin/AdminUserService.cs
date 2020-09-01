using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminUserService
    {
        public Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex);
        public Task<ApplicationUserEditDto> GetUserEditAsync(string id);
        public Task<ApplicationUserEditDto> UpdateUserAsync(string id, ApplicationUserEditDto dto);
        public Task DeleteUserAsync(string id);
    }

    public class AdminUserService : IAdminUserService
    {
        private const int PageSize = 50;

        private readonly UserManager<ApplicationUser> _manager;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(UserManager<ApplicationUser> manager, ILogger<AdminUserService> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        private async Task EnsureUserExists(string id)
        {
            var user = await _manager.FindByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateApplicationUserEditDto(string id, ApplicationUserEditDto dto)
        {
            if (await _manager.Users.AnyAsync(u => u.Id != id && u.ContestantId == dto.ContestantId))
            {
                throw new ValidationException("Contestant ID already taken.");
            }
        }

        public async Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex)
        {
            return await _manager.Users.PaginateAsync(u => new ApplicationUserInfoDto(u), pageIndex ?? 1, PageSize);
        }

        public async Task<ApplicationUserEditDto> GetUserEditAsync(string id)
        {
            await EnsureUserExists(id);
            var user = await _manager.FindByIdAsync(id);
            var roles = await _manager.GetRolesAsync(user);
            return new ApplicationUserEditDto(user, roles);
        }

        public async Task<ApplicationUserEditDto> UpdateUserAsync(string id, ApplicationUserEditDto dto)
        {
            await EnsureUserExists(id);
            await ValidateApplicationUserEditDto(id, dto);

            var user = await _manager.FindByIdAsync(id);
            user.ContestantId = dto.ContestantId;
            user.ContestantName = dto.ContestantName;
            await _manager.UpdateAsync(user);

            var pairs = new List<KeyValuePair<bool, string>>
            {
                new KeyValuePair<bool, string>(dto.IsAdministrator.GetValueOrDefault(), ApplicationRoles.Administrator),
                new KeyValuePair<bool, string>(dto.IsUserManager.GetValueOrDefault(), ApplicationRoles.UserManager),
                new KeyValuePair<bool, string>
                    (dto.IsContestManager.GetValueOrDefault(), ApplicationRoles.ContestManager),
                new KeyValuePair<bool, string>
                    (dto.IsSubmissionManager.GetValueOrDefault(), ApplicationRoles.SubmissionManager)
            };
            foreach (var pair in pairs)
            {
                if (pair.Key)
                {
                    if (!await _manager.IsInRoleAsync(user, pair.Value))
                    {
                        await _manager.AddToRoleAsync(user, pair.Value);
                    }
                }
                else
                {
                    if (await _manager.IsInRoleAsync(user, pair.Value))
                    {
                        await _manager.RemoveFromRoleAsync(user, pair.Value);
                    }
                }
            }

            return new ApplicationUserEditDto(user, await _manager.GetRolesAsync(user));
        }

        public async Task DeleteUserAsync(string id)
        {
            await EnsureUserExists(id);
            var user = await _manager.FindByIdAsync(id);
            var roles = await _manager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                throw new ValidationException("Cannot delete user with some roles.");
            }

            await _manager.DeleteAsync(user);
        }
    }
}