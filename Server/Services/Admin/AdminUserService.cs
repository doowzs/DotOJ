using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Exceptions;

namespace Server.Services.Admin
{
    public interface IAdminUserService
    {
        public Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex);
        public Task<ApplicationUserEditDto> GetUserEditAsync(string id);
        public Task<ApplicationUserEditDto> UpdateUserAsync(string id, ApplicationUserEditDto dto);
        public Task<string> ImportUsersAsync(string input);
        public Task DeleteUserAsync(string id);
    }

    public class AdminUserService : LoggableService<AdminUserService>, IAdminUserService
    {
        private const int PageSize = 50;

        public AdminUserService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureUserExists(string id)
        {
            var user = await Manager.FindByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException();
            }
        }

        private async Task ValidateApplicationUserEditDto(string id, ApplicationUserEditDto dto)
        {
            if (await Manager.Users.AnyAsync(u => u.Id != id && u.ContestantId == dto.ContestantId))
            {
                throw new ValidationException("Contestant ID already taken.");
            }

            if (dto.ContestantId.Length > 50)
            {
                throw new ValidationException("Contestant ID must be shorter than 50 characters.");
            }

            if (dto.ContestantName.Length > 20)
            {
                throw new ValidationException("Contestant Name must be shorter than 20 characters.");
            }
        }

        public async Task<PaginatedList<ApplicationUserInfoDto>> GetPaginatedUserInfosAsync(int? pageIndex)
        {
            return await Manager.Users
                .OrderBy(u => u.ContestantId)
                .PaginateAsync(u => new ApplicationUserInfoDto(u), pageIndex ?? 1, PageSize);
        }

        public async Task<ApplicationUserEditDto> GetUserEditAsync(string id)
        {
            await EnsureUserExists(id);
            var user = await Manager.FindByIdAsync(id);
            var roles = await Manager.GetRolesAsync(user);
            return new ApplicationUserEditDto(user, roles);
        }

        public async Task<ApplicationUserEditDto> UpdateUserAsync(string id, ApplicationUserEditDto dto)
        {
            await EnsureUserExists(id);
            await ValidateApplicationUserEditDto(id, dto);

            var user = await Manager.FindByIdAsync(id);
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var token = await Manager.GeneratePasswordResetTokenAsync(user);
                var result = await Manager.ResetPasswordAsync(user, token, dto.Password);
                if (!result.Succeeded)
                {
                    throw new ValidationException(string.Join(',', result.Errors.Select(e => e.Description)));
                }
            }

            user.ContestantId = dto.ContestantId;
            user.ContestantName = dto.ContestantName;
            await Manager.UpdateAsync(user);

            var pairs = new List<KeyValuePair<bool, string>>
            {
                new KeyValuePair<bool, string>(dto.IsAdministrator.GetValueOrDefault(), ApplicationRoles.Administrator),
                new KeyValuePair<bool, string>(dto.IsUserManager.GetValueOrDefault(), ApplicationRoles.UserManager),
                new KeyValuePair<bool, string>
                    (dto.IsContestManager.GetValueOrDefault(), ApplicationRoles.ContestManager),
                new KeyValuePair<bool, string>
                    (dto.IsSubmissionManager.GetValueOrDefault(), ApplicationRoles.SubmissionManager)
            };

            var roles = new List<string>();
            foreach (var pair in pairs)
            {
                if (pair.Key)
                {
                    roles.Add(pair.Value);
                    if (!await Manager.IsInRoleAsync(user, pair.Value))
                    {
                        await Manager.AddToRoleAsync(user, pair.Value);
                    }
                }
                else
                {
                    if (await Manager.IsInRoleAsync(user, pair.Value))
                    {
                        await Manager.RemoveFromRoleAsync(user, pair.Value);
                    }
                }
            }

            await LogInformation($"UpdateUser Id={user.Id} ContestantId={user.ContestantId} " +
                                 $"ContestantName={user.ContestantName} Roles={roles}");
            return new ApplicationUserEditDto(user, await Manager.GetRolesAsync(user));
        }

        public async Task<string> ImportUsersAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new BadHttpRequestException("Invalid input string.");
            }
            
            var results = new StringBuilder("Id,ContestantId,ContestantName,Email,Password\n");
            var lines = input.Split('\n');
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var user = new ApplicationUser
                    {
                        UserName = parts[0],
                        ContestantId = parts[0],
                        ContestantName = parts[1],
                        Email = parts[2]
                    };
                    var password = parts.Length >= 4 ? parts[3] : parts[0];
                    var result = await Manager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        Logger.LogInformation($"Admin import account Id={user.Id} ContestantId={user.ContestantId}.");
                        results.AppendLine(
                            $"{user.Id},{user.ContestantId},{user.ContestantName},{user.Email},{password}");
                    }
                    else
                    {
                        results.AppendLine($"null,{string.Join(',', result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            return results.ToString();
        }

        public async Task DeleteUserAsync(string id)
        {
            await EnsureUserExists(id);
            var user = await Manager.FindByIdAsync(id);
            var roles = await Manager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                throw new ValidationException("Cannot delete user with some roles.");
            }

            await LogInformation($"DeleteUser Id={id}");
            await Manager.DeleteAsync(user);
        }
    }
}