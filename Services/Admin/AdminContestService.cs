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
        public Task<List<RegistrationInfoDto>> GetRegistrationsAsync(int id);
        public Task<List<RegistrationInfoDto>> AddRegistrationsAsync(int id, IEnumerable<string> userIds);
        public Task RemoveRegistrationsAsync(int id, IEnumerable<string> userIds);
        public Task<List<RegistrationInfoDto>> CopyRegistrationsAsync(int to, int from);
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

        private async Task EnsureContestExistsAsync(int id)
        {
            if (!await _context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private Task ValidateContestEditDtoAsync(ContestEditDto dto)
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
            await EnsureContestExistsAsync(id);
            return new ContestEditDto(await _context.Contests.FindAsync(id));
        }

        public async Task<ContestEditDto> CreateContestAsync(ContestEditDto dto)
        {
            await ValidateContestEditDtoAsync(dto);
            var contest = new Contest
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
            await EnsureContestExistsAsync(id);
            await ValidateContestEditDtoAsync(dto);
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
            await EnsureContestExistsAsync(id);
            var contest = new Contest {Id = id};
            _context.Contests.Attach(contest);
            _context.Contests.Remove(contest);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RegistrationInfoDto>> GetRegistrationsAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            return await _context.Registrations
                .Where(r => r.ContestId == id)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }

        public async Task<List<RegistrationInfoDto>> AddRegistrationsAsync(int id, IEnumerable<string> userIds)
        {
            await EnsureContestExistsAsync(id);
            var registrations = new List<RegistrationInfoDto>();
            foreach (var userId in userIds)
            {
                var registered =
                    await _context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == userId);
                if (registered)
                {
                    var registration = await _context.Registrations.FindAsync(userId, id);
                    await _context.Entry(registration).Reference(r => r.User).LoadAsync();
                    registrations.Add(new RegistrationInfoDto(registration));
                }
                else if (await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    var registration = new Registration
                    {
                        ContestId = id,
                        UserId = userId,
                        IsContestManager = false,
                        IsParticipant = true,
                        Statistics = new List<ProblemStatistics>()
                    };
                    await _context.Registrations.AddAsync(registration);

                    await _context.Entry(registration).Reference(r => r.User).LoadAsync();
                    registrations.Add(new RegistrationInfoDto(registration));
                }
            }

            await _context.SaveChangesAsync();
            return registrations;
        }

        public async Task RemoveRegistrationsAsync(int id, IEnumerable<string> userIds)
        {
            await EnsureContestExistsAsync(id);
            foreach (var userId in userIds)
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
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<RegistrationInfoDto>> CopyRegistrationsAsync(int to, int from)
        {
            if (to == from)
            {
                throw new ValidationException("Two contests cannot be the same.");
            }

            await EnsureContestExistsAsync(to);
            await EnsureContestExistsAsync(from);

            List<Registration> registrations;

            registrations = await _context.Registrations.Where(r => r.ContestId == to).ToListAsync();
            _context.Registrations.RemoveRange(registrations);
            await _context.SaveChangesAsync();

            registrations = await _context.Registrations
                .Where(r => r.ContestId == from)
                .Select(r => new Registration
                {
                    ContestId = to,
                    UserId = r.UserId,
                    IsParticipant = r.IsParticipant,
                    IsContestManager = r.IsContestManager,
                    Statistics = new List<ProblemStatistics>()
                })
                .ToListAsync();
            await _context.Registrations.AddRangeAsync(registrations);
            await _context.SaveChangesAsync();

            return await _context.Registrations
                .Where(r => r.ContestId == to)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }
    }
}