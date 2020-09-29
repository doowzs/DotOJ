using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs;
using Data.Generics;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApp.Exceptions;

namespace WebApp.Services
{
    public interface IContestService
    {
        public Task<List<ContestInfoDto>> GetCurrentContestInfosAsync();
        public Task<PaginatedList<ContestInfoDto>> GetPaginatedContestInfosAsync(int? pageIndex);
        public Task<ContestViewDto> GetContestViewAsync(int id);
        public Task<List<RegistrationInfoDto>> GetRegistrationInfosAsync(int id);
    }

    public class ContestService : LoggableService<ContestService>, IContestService
    {
        private const int PageSize = 20;

        public ContestService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureContestExistsAsync(int id)
        {
            if (!await Context.Contests.AnyAsync(c => c.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewContestAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            var contest = await Context.Contests.FindAsync(id);
            if (contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this contest.");
                }
            }
            else
            {
                var registered = await Context.Registrations
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
            var contests = await Context.Contests
                .Where(c => c.EndTime > now)
                .OrderBy(c => c.BeginTime)
                .ToListAsync();
            if (Accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = Accessor.HttpContext.User.GetSubjectId();
                var infos = new List<ContestInfoDto>();
                foreach (var contest in contests)
                {
                    var registered = await Context.Registrations
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
            var contests = await Context.Contests
                .OrderByDescending(c => c.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<ContestInfoDto> infos;
            if (Accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = Accessor.HttpContext.User.GetSubjectId();
                infos = new List<ContestInfoDto>();
                foreach (var contest in contests.Items)
                {
                    var registered = await Context.Registrations
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

            var contest = await Context.Contests.FindAsync(id);
            if (contest is null)
            {
                throw new NotFoundException();
            }

            await Context.Entry(contest).Collection(c => c.Problems).LoadAsync();

            IList<ProblemInfoDto> problemInfos;
            if (Accessor.HttpContext.User.IsAuthenticated())
            {
                var userId = Accessor.HttpContext.User.GetSubjectId();
                problemInfos = new List<ProblemInfoDto>();
                foreach (var problem in contest.Problems)
                {
                    var query = Context.Submissions.Where(s => s.ProblemId == problem.Id);
                    var attempted = await query.AnyAsync(s => s.UserId == userId);
                    var solved = await query.AnyAsync(s => s.UserId == userId && s.Verdict == Verdict.Accepted);
                    var acceptedSubmissions = await query.CountAsync(s => s.Verdict == Verdict.Accepted);
                    var totalSubmissions = await query.CountAsync();
                    problemInfos.Add(new ProblemInfoDto(problem, attempted, solved, acceptedSubmissions, totalSubmissions));
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

            return await Context.Registrations
                .Where(r => r.ContestId == id)
                .Include(r => r.User)
                .Select(r => new RegistrationInfoDto(r))
                .ToListAsync();
        }
    }
}