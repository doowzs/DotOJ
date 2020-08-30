using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services.Admin
{
    public interface IAdminContestService
    {
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestEditDto> GetContestEditAsync(int id);
        public Task<ContestEditDto> CreateContestAsync(ContestEditDto dto);
        public Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto);
        public Task DeleteContestAsync(int id);
        public Task RegisterUserForContestAsync(int id, string userId);
        public Task UnregisterUserFromContestAsync(int id, string userId);
    }

    public class AdminContestService : IAdminContestService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminContestService> _logger;

        public AdminContestService(ApplicationDbContext context, ILogger<AdminContestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task EnsureContestExists(int id)
        {
            if (!await _context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private Task ValidateContestEditDto(ContestEditDto dto)
        {
            if (string.IsNullOrEmpty(dto.Title))
            {
                throw new ValidationException("Title cannot be empty.");
            }

            if (!Enum.IsDefined(typeof(ContestMode), dto.Mode.GetValueOrDefault()))
            {
                throw new ValidationException("Invalid contest mode.");
            }

            if (dto.BeginTime >= dto.EndTime)
            {
                throw new ValidationException("Invalid begin and end time.");
            }

            return Task.CompletedTask;
        }


        public async Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex)
        {
            var contests = await _context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            var infos = contests.Items.Select(c => new ContestInfoDto(c, false)).ToList();
            return new PaginatedList<ContestInfoDto>(contests.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ContestEditDto> GetContestEditAsync(int id)
        {
            await EnsureContestExists(id);
            return new ContestEditDto(await _context.Contests.FindAsync(id));
        }

        public async Task<ContestEditDto> CreateContestAsync(ContestEditDto dto)
        {
            await ValidateContestEditDto(dto);
            var contest = new Contest()
            {
                Title = dto.Title,
                Description = dto.Description,
                IsPublic = dto.IsPublic.GetValueOrDefault(),
                Mode = dto.Mode.GetValueOrDefault(),
                BeginTime = dto.BeginTime,
                EndTime = dto.EndTime
            };
            await _context.Contests.AddAsync(contest);
            await _context.SaveChangesAsync();
            return new ContestEditDto(contest);
        }

        public async Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto)
        {
            await EnsureContestExists(id);
            await ValidateContestEditDto(dto);
            var contest = await _context.Contests.FindAsync(id);
            contest.Title = dto.Title;
            contest.Description = dto.Description;
            contest.IsPublic = dto.IsPublic.GetValueOrDefault();
            contest.Mode = dto.Mode.GetValueOrDefault();
            contest.BeginTime = dto.BeginTime;
            contest.EndTime = dto.EndTime;
            _context.Contests.Update(contest);
            await _context.SaveChangesAsync();
            return new ContestEditDto(contest);
        }

        public async Task DeleteContestAsync(int id)
        {
            await EnsureContestExists(id);
            var contest = new Contest {Id = id};
            _context.Contests.Attach(contest);
            _context.Contests.Remove(contest);
            await _context.SaveChangesAsync();
        }

        public async Task RegisterUserForContestAsync(int id, string userId)
        {
            var registered =
                await _context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == userId);
            if (!registered)
            {
                var registration = new Registration
                {
                    ContestId = id,
                    UserId = userId,
                    IsContestManager = false,
                    IsParticipant = false,
                    Statistics = new List<ProblemStatistics>()
                };
                await _context.Registrations.AddAsync(registration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnregisterUserFromContestAsync(int id, string userId)
        {
            var registered =
                await _context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == userId);
            if (registered)
            {
                var registration = new Registration
                {
                    ContestId = id,
                    UserId = userId,
                };
                _context.Registrations.Attach(registration);
                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
            }
        }
    }
}