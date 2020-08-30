using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IContestService
    {
        public Task<List<ContestInfoDto>> GetCurrentContestInfosAsync(string userId);
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex, string userId);
        public Task<ContestViewDto> GetContestViewAsync(int id, string userId);
        public Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id, string userId);
    }

    public class ContestService : IContestService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly ILogger<ContestService> _logger;

        public ContestService
            (ApplicationDbContext context, UserManager<ApplicationUser> manager, ILogger<ContestService> logger)
        {
            _context = context;
            _manager = manager;
            _logger = logger;
        }

        private async Task<bool> CanViewContest(int id, string userId)
        {
            var user = await _manager.FindByIdAsync(userId);
            if (await _manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await _manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return true;
            }

            var contest = await _context.Contests.FindAsync(id);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    return false;
                }
            }
            else
            {
                var registered = await _context.Registrations
                    .AnyAsync(r => r.ContestId == contest.Id && r.UserId == userId);
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < contest.EndTime))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task ValidateContestId(int id)
        {
            if (!await _context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new ValidationException("Invalid contest ID.");
            }
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
            await ValidateContestId(id);
            if (!await CanViewContest(id, userId))
            {
                throw new UnauthorizedAccessException("Not authorized to view this contest.");
            }

            var contest = await _context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
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

        public async Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id, string userId)
        {
            await ValidateContestId(id);
            if (!await CanViewContest(id, userId))
            {
                throw new UnauthorizedAccessException("Not authorized to view this contest.");
            }

            return await _context.Registrations
                .Where(r => r.ContestId == id)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }
    }
}