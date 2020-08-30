using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IContestService
    {
        public Task<List<ContestInfoDto>> GetCurrentContestInfosAsync();
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestViewDto> GetContestViewAsync(int id);
        public Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id);
    }

    public class ContestService : IContestService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<ContestService> _logger;

        public ContestService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IHttpContextAccessor accessor, ILogger<ContestService> logger)
        {
            _context = context;
            _manager = manager;
            _accessor = accessor;
            _logger = logger;
        }

        private async Task EnsureContestExistsAsync(int id)
        {
            if (!await _context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewContestAsync(int id)
        {
            var user = await _manager.GetUserAsync(_accessor.HttpContext.User);
            if (await _manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await _manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            var contest = await _context.Contests.FindAsync(id);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this contest.");
                }
            }
            else
            {
                var registered = await _context.Registrations
                    .AnyAsync(r => r.ContestId == contest.Id && r.UserId == user.Id);
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < contest.EndTime))
                {
                    throw new UnauthorizedAccessException("Not authorized to view this contest.");
                }
            }
        }

        public async Task<List<ContestInfoDto>> GetCurrentContestInfosAsync()
        {
            var now = DateTime.Now.ToUniversalTime();
            var contests = await _context.Contests
                .Where(c => c.EndTime > now)
                .OrderBy(c => c.BeginTime)
                .ToListAsync();
            if (_accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = _accessor.HttpContext.User.GetSubjectId();
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

        public async Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex)
        {
            // See https://github.com/dotnet/efcore/issues/17068 for GroupJoin issues.
            var contests = await _context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<ContestInfoDto> infos;
            if (_accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = _accessor.HttpContext.User.GetSubjectId();
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

        public async Task<ContestViewDto> GetContestViewAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            await EnsureUserCanViewContestAsync(id);

            var contest = await _context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
            }

            await _context.Entry(contest).Collection(c => c.Problems).LoadAsync();
            await _context.Entry(contest).Collection(c => c.Clarifications).LoadAsync();

            IList<ProblemInfoDto> problemInfos;
            if (_accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = _accessor.HttpContext.User.GetSubjectId();
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

        public async Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id)
        {
            await EnsureContestExistsAsync(id);
            await EnsureUserCanViewContestAsync(id);

            return await _context.Registrations
                .Where(r => r.ContestId == id)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }
    }
}