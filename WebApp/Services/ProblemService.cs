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
    public interface IProblemService
    {
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex);
        public Task<ProblemViewDto> GetProblemViewAsync(int id);
    }

    public class ProblemService : LoggableService<ProblemService>, IProblemService
    {
        private const int PageSize = 50;

        public ProblemService(IServiceProvider provider) : base(provider)
        {
        }

        private async Task EnsureProblemExistsAsync(int id)
        {
            if (!await Context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewProblemAsync(int id)
        {
            var user = await Manager.GetUserAsync(Accessor.HttpContext.User);
            if (await Manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await Manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            var problem = await Context.Problems.FindAsync(id);
            await Context.Entry(problem).Reference<Contest>(p => p.Contest).LoadAsync();
            if (problem.Contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this problem.");
                }
            }
            else
            {
                var registered = await Context.Registrations
                    .AnyAsync(r => r.ContestId == problem.Contest.Id && r.UserId == user.Id);
                if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime ||
                    (!registered && DateTime.Now.ToUniversalTime() < problem.Contest.EndTime))
                {
                    throw new UnauthorizedAccessException("Not authorized to view this problem.");
                }
            }
        }

        public async Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex)
        {
            var userId = Accessor.HttpContext.User.GetSubjectId();
            var problems = await Context.Problems.PaginateAsync(pageIndex ?? 1, PageSize);
            var infos = new List<ProblemInfoDto>();
            foreach (var problem in problems.Items)
            {
                var query = Context.Submissions.Where(s => s.ProblemId == problem.Id);
                var attempted = await query.AnyAsync(s => s.UserId == userId);
                var solved = await query.AnyAsync(s => s.UserId == userId && s.Verdict == Verdict.Accepted);
                var acceptedSubmissions = await query.CountAsync(s => s.Verdict == Verdict.Accepted);
                var totalSubmissions = await query.CountAsync();
                infos.Add(new ProblemInfoDto(problem, attempted, solved, acceptedSubmissions, totalSubmissions));
            }

            return new PaginatedList<ProblemInfoDto>(problems.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ProblemViewDto> GetProblemViewAsync(int id)
        {
            await EnsureProblemExistsAsync(id);
            await EnsureUserCanViewProblemAsync(id);

            var userId = Accessor.HttpContext.User.GetSubjectId();
            var problem = await Context.Problems.FindAsync(id);
            await Context.Entry(problem).Collection(p => p.Submissions).LoadAsync();
            var attempted = problem.Submissions.Any(s => s.UserId == userId);
            var solved = problem.Submissions.Any(s => s.UserId == userId && s.Verdict == Verdict.Accepted);
            var acceptedSubmissions = problem.Submissions.Count(s => s.Verdict == Verdict.Accepted);
            var totalSubmissions = problem.Submissions.Count;
            return new ProblemViewDto(problem, attempted, solved, acceptedSubmissions, totalSubmissions);
        }
    }
}