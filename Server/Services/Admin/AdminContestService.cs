using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using Server.Exceptions;

namespace Server.Services.Admin
{
    public interface IAdminContestService
    {
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestEditDto> GetContestEditAsync(int id);
        public Task<ContestEditDto> CreateContestAsync(ContestEditDto dto);
        public Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto);
        public Task DeleteContestAsync(int id);
        public Task<List<RegistrationInfoDto>> GetRegistrationsAsync(int id);

        public Task<List<RegistrationInfoDto>> AddRegistrationsAsync
            (int id, IList<string> userIds, bool isParticipant, bool isContestManager);

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

            if (dto.HasScoreBonus.GetValueOrDefault(false))
            {
                if (!dto.ScoreBonusTime.HasValue)
                {
                    throw new ValidationException("Score bonus time cannot be null.");
                }
                else if (dto.ScoreBonusTime.Value < dto.BeginTime)
                {
                    throw new ValidationException("Score bonus time cannot be before contest start time.");
                }
                else if (dto.ScoreBonusTime.Value > dto.EndTime)
                {
                    throw new ValidationException("Score bonus time cannot be after contest end time.");
                }

                switch (dto.ScoreBonusPercentage)
                {
                    case null:
                        throw new ValidationException("Score bonus percentage cannot be null.");
                    case < 100:
                        throw new ValidationException("Score bonus percentage cannot be less than 100.");
                }
            }

            if (dto.HasScoreDecay.GetValueOrDefault(false))
            {
                if (!dto.ScoreDecayTime.HasValue)
                {
                    throw new ValidationException("Score decay time cannot be null.");
                }
                else if (dto.ScoreDecayTime.Value < dto.BeginTime)
                {
                    throw new ValidationException("Score decay time cannot be before contest start time.");
                }
                else if (dto.ScoreDecayTime.Value > dto.EndTime)
                {
                    throw new ValidationException("Score decay time cannot be after contest end time.");
                }

                if (!dto.IsScoreDecayLinear.HasValue)
                {
                    throw new ValidationException("Score decay linear cannot be null.");
                }
                switch (dto.ScoreDecayPercentage)
                {
                    case null:
                        throw new ValidationException("Score decay percentage cannot be null.");
                    case < 0:
                        throw new ValidationException("Score decay percentage cannot be negative.");
                    case > 100:
                        throw new ValidationException("Score decay percentage cannot be more than 100.");
                }
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
                EndTime = dto.EndTime,
                HasScoreBonus = dto.HasScoreBonus.GetValueOrDefault(),
                ScoreBonusTime = dto.ScoreBonusTime,
                ScoreBonusPercentage = dto.ScoreBonusPercentage,
                HasScoreDecay = dto.HasScoreDecay.GetValueOrDefault(),
                IsScoreDecayLinear = dto.IsScoreDecayLinear,
                ScoreDecayTime = dto.ScoreDecayTime,
                ScoreDecayPercentage = dto.ScoreDecayPercentage
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
            contest.HasScoreBonus = dto.HasScoreBonus.GetValueOrDefault();
            contest.ScoreBonusTime = dto.ScoreBonusTime;
            contest.ScoreBonusPercentage = dto.ScoreBonusPercentage;
            contest.HasScoreDecay = dto.HasScoreDecay.GetValueOrDefault();
            contest.IsScoreDecayLinear = dto.IsScoreDecayLinear;
            contest.ScoreDecayTime = dto.ScoreDecayTime;
            contest.ScoreDecayPercentage = dto.ScoreDecayPercentage;
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

        public async Task<List<RegistrationInfoDto>> AddRegistrationsAsync
            (int id, IList<string> userIds, bool isParticipant, bool isContestManager)
        {
            await EnsureContestExistsAsync(id);

            var registrations = await Context.Registrations
                .Where(r => r.ContestId == id && userIds.Contains(r.UserId))
                .ToListAsync();
            Context.Registrations.RemoveRange(registrations);
            await Context.SaveChangesAsync();

            registrations = userIds.Select(userId => new Registration
            {
                ContestId = id,
                UserId = userId,
                IsParticipant = isParticipant,
                IsContestManager = isContestManager,
                Statistics = new List<RegistrationProblemStatistics>()
            }).ToList();
            await Context.Registrations.AddRangeAsync(registrations);
            foreach (var registration in registrations)
            {
                await registration.RebuildStatisticsAsync(Context);
                await Context.Entry(registration).Reference(r => r.User).LoadAsync();
            }

            await Context.SaveChangesAsync();
            await LogInformation($"AddRegistrations Contest={id} Users={string.Join(",", userIds)}");
            return registrations.Select(r => new RegistrationInfoDto(r)).ToList();
        }

        public async Task RemoveRegistrationsAsync(int id, IList<string> userIds)
        {
            await EnsureContestExistsAsync(id);
            var registrations = await Context.Registrations
                .Where(r => r.ContestId == id && userIds.Contains(r.UserId))
                .ToListAsync();
            Context.Registrations.RemoveRange(registrations);
            await Context.SaveChangesAsync();
            await LogInformation($"RemoveRegistrations Contest={id} Users={string.Join(",", userIds)}");
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
                    Statistics = new List<RegistrationProblemStatistics>()
                })
                .ToListAsync();
            await Context.Registrations.AddRangeAsync(registrations);
            foreach (var registration in registrations)
            {
                await registration.RebuildStatisticsAsync(Context);
            }

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