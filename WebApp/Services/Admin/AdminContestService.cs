using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs;
using Data.Generics;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.Services.Admin
{
    public interface IAdminContestService
    {
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestEditDto> GetContestEditAsync(int id);
        public Task<ContestEditDto> CreateContestAsync(ContestEditDto dto);
        public Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto);
        public Task DeleteContestAsync(int id);
        public Task<List<RegistrationInfoDto>> GetRegistrationsAsync(int id);
        public Task<List<RegistrationInfoDto>> AddRegistrationsAsync(int id, IList<string> userIds);
        public Task RemoveRegistrationsAsync(int id, IList<string> userIds);
        public Task<List<RegistrationInfoDto>> CopyRegistrationsAsync(int to, int from);
    }

    public class AdminContestService : LoggableService<AdminContestService>, IAdminContestService
    {
        private const int PageSize = 20;

        public AdminContestService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureContestExistsAsync(int id)
        {
            if (!await Context.Contests.AnyAsync(c => c.Id == id))
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
            return await Context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(c => new ContestInfoDto(c, false), pageIndex ?? 1, PageSize);
        }

        public async Task<ContestEditDto> GetContestEditAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            return new ContestEditDto(await Context.Contests.FindAsync(id));
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
            await Context.Contests.AddAsync(contest);
            await Context.SaveChangesAsync();

            await LogInformation($"UpdateContest Id={contest.Id} Title={contest.Title} " +
                                 $"IsPublic={contest.IsPublic} Mode={contest.Mode}");
            return new ContestEditDto(contest);
        }

        public async Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto)
        {
            await EnsureContestExistsAsync(id);
            await ValidateContestEditDtoAsync(dto);
            var contest = await Context.Contests.FindAsync(id);
            contest.Title = dto.Title;
            contest.Description = dto.Description;
            contest.IsPublic = dto.IsPublic.GetValueOrDefault();
            contest.Mode = dto.Mode.GetValueOrDefault();
            contest.BeginTime = dto.BeginTime;
            contest.EndTime = dto.EndTime;
            Context.Contests.Update(contest);
            await Context.SaveChangesAsync();

            await LogInformation($"UpdateContest Id={contest.Id} Title={contest.Title} " +
                                 $"IsPublic={contest.IsPublic} Mode={contest.Mode}");
            return new ContestEditDto(contest);
        }

        public async Task DeleteContestAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            var contest = new Contest {Id = id};
            Context.Contests.Attach(contest);
            Context.Contests.Remove(contest);
            await Context.SaveChangesAsync();
            await LogInformation($"DeleteContest Id={id}");
        }

        public async Task<List<RegistrationInfoDto>> GetRegistrationsAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            return await Context.Registrations
                .Where(r => r.ContestId == id)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }

        public async Task<List<RegistrationInfoDto>> AddRegistrationsAsync(int id, IList<string> userIds)
        {
            await EnsureContestExistsAsync(id);
            var registrations = new List<RegistrationInfoDto>();
            foreach (var userId in userIds)
            {
                var registered =
                    await Context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == userId);
                if (registered)
                {
                    var registration = await Context.Registrations.FindAsync(userId, id);
                    await Context.Entry(registration).Reference(r => r.User).LoadAsync();
                    registrations.Add(new RegistrationInfoDto(registration));
                }
                else if (await Context.Users.AnyAsync(u => u.Id == userId))
                {
                    var registration = new Registration
                    {
                        ContestId = id,
                        UserId = userId,
                        IsContestManager = false,
                        IsParticipant = true,
                        Statistics = new List<ProblemStatistics>()
                    };
                    await Context.Registrations.AddAsync(registration);

                    await Context.Entry(registration).Reference(r => r.User).LoadAsync();
                    registrations.Add(new RegistrationInfoDto(registration));
                }
            }

            await Context.SaveChangesAsync();
            await LogInformation($"AddRegistrations Contest={id} Users={string.Join(",", userIds)}");
            return registrations;
        }

        public async Task RemoveRegistrationsAsync(int id, IList<string> userIds)
        {
            await EnsureContestExistsAsync(id);
            foreach (var userId in userIds)
            {
                var registered =
                    await Context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == userId);
                if (registered)
                {
                    var registration = new Registration
                    {
                        ContestId = id,
                        UserId = userId,
                    };
                    Context.Registrations.Attach(registration);
                    Context.Registrations.Remove(registration);
                }
            }

            await LogInformation($"RemoveRegistrations Contest={id} Users={string.Join(",", userIds)}");
            await Context.SaveChangesAsync();
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

            registrations = await Context.Registrations.Where(r => r.ContestId == to).ToListAsync();
            Context.Registrations.RemoveRange(registrations);
            await Context.SaveChangesAsync();

            registrations = await Context.Registrations
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
            await Context.Registrations.AddRangeAsync(registrations);
            await Context.SaveChangesAsync();

            await LogInformation($"CopyRegistrations To={to} From={from} Copied={registrations.Count}");

            return await Context.Registrations
                .Where(r => r.ContestId == to)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }
    }
}