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

namespace Judge1.Services
{
    public interface IContestService
    {
        public Task ValidateContestId(int id);
        public Task ValidateContestEditDto(ContestEditDto dto);
        public Task<List<ContestInfoDto>> GetCurrentContestInfosAsync(string userId);
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex, string userId);
        public Task<ContestViewDto> GetContestViewAsync(int id, string userId);
        public Task<ContestEditDto> GetContestEditAsync(int id);
        public Task<ContestEditDto> CreateContestAsync(ContestEditDto dto);
        public Task<ContestEditDto> UpdateContestAsync(int id, ContestEditDto dto);
        public Task DeleteContestAsync(int id);
        public Task<PaginatedList<RegistrationDto>> GetPaginatedRegistrationsAsync(int id, int? pageIndex);
        public Task RegisterUserForContestAsync(int id, ApplicationUser user);
        public Task UnregisterUserFromContestAsync(int id, ApplicationUser user);
    }

    public class ContestService : IContestService
    {
        private const int PageSize = 20;
        private const int RegistrationPageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContestService> _logger;

        public ContestService(ApplicationDbContext context, ILogger<ContestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ValidateContestId(int id)
        {
            if (!await _context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new ValidationException("Invalid contest ID.");
            }
        }

        public Task ValidateContestEditDto(ContestEditDto dto)
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

        public async Task<List<ContestInfoDto>> GetCurrentContestInfosAsync(string userId)
        {
            var now = DateTime.Now.ToUniversalTime();
            var contests = await _context.Contests
                .Where(c => c.EndTime > now)
                .OrderBy(c => c.BeginTime)
                .ToListAsync();
            if (userId != null)
            {
                var infos = new List<ContestInfoDto>();
                foreach (var contest in contests)
                {
                    var registered = await _context.Registrations
                        .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                    infos.Add(new ContestInfoDto(contest, registered));
                }

                return infos;
            }
            else
            {
                return contests.Select(c => new ContestInfoDto(c, false)).ToList();
            }
        }

        public async Task<PaginatedList<ContestInfoDto>>
            GetPaginatedContestInfosAsync(int? pageIndex, string userId)
        {
            // See https://github.com/dotnet/efcore/issues/17068 for GroupJoin issues.
            var contests = await _context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<ContestInfoDto> infos;
            if (userId != null)
            {
                infos = new List<ContestInfoDto>();
                foreach (var contest in contests.Items)
                {
                    var registered = await _context.Registrations
                        .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                    infos.Add(new ContestInfoDto(contest, registered));
                }
            }
            else
            {
                infos = contests.Items.Select(c => new ContestInfoDto(c, false)).ToList();
            }

            return new PaginatedList<ContestInfoDto>(contests.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ContestViewDto> GetContestViewAsync(int id, string userId)
        {
            var contest = await _context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
            }

            if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
            {
                throw new UnauthorizedAccessException("Not authorized to view this contest.");
            }

            await _context.Entry(contest).Collection(c => c.Problems).LoadAsync();
            await _context.Entry(contest).Collection(c => c.Clarifications).LoadAsync();

            IList<ProblemInfoDto> problemInfos;
            if (userId != null)
            {
                problemInfos = new List<ProblemInfoDto>();
                foreach (var problem in contest.Problems)
                {
                    var solved = await _context.Submissions
                        .AnyAsync(s => s.ProblemId == problem.Id && s.UserId == userId && s.FailedOn == -1);
                    problemInfos.Add(new ProblemInfoDto(problem, solved));
                }
            }
            else
            {
                problemInfos = contest.Problems.Select(p => new ProblemInfoDto(p)).ToList();
            }
            
            return new ContestViewDto(contest, problemInfos);
        }

        public async Task<ContestEditDto> GetContestEditAsync(int id)
        {
            var contest = await _context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
            }

            return new ContestEditDto(contest);
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
            await ValidateContestId(id);
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
            await ValidateContestId(id);
            var contest = new Contest {Id = id};
            _context.Contests.Attach(contest);
            _context.Contests.Remove(contest);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedList<RegistrationDto>>
            GetPaginatedRegistrationsAsync(int id, int? pageIndex)
        {
            return await _context.Registrations
                .Where(r => r.ContestId == id)
                .PaginateAsync(r => new RegistrationDto(r), pageIndex ?? 1, RegistrationPageSize);
        }

        public async Task RegisterUserForContestAsync(int id, ApplicationUser user)
        {
            var registered =
                await _context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == user.Id);
            if (!registered)
            {
                var registration = new Registration
                {
                    ContestId = id,
                    UserId = user.Id,
                    IsContestManager = false,
                    IsParticipant = false,
                    Statistics = new ParticipantStatistics()
                };
                await _context.Registrations.AddAsync(registration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnregisterUserFromContestAsync(int id, ApplicationUser user)
        {
            var registered =
                await _context.Registrations.AnyAsync(r => r.ContestId == id && r.UserId == user.Id);
            if (registered)
            {
                var registration = new Registration
                {
                    ContestId = id,
                    UserId = user.Id,
                };
                _context.Registrations.Attach(registration);
                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
            }
        }
    }
}