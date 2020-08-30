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
    public interface IProblemService
    {
        public Task<PaginatedList<ProblemInfoDto>> GetPaginatedProblemInfosAsync(int? pageIndex);
        public Task<ProblemViewDto> GetProblemViewAsync(int id);
    }

    public class ProblemService : IProblemService
    {
        private const int PageSize = 50;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<ProblemService> _logger;

        public ProblemService(ApplicationDbContext context, UserManager<ApplicationUser> manager,
            IHttpContextAccessor accessor, ILogger<ProblemService> logger)
        {
            _context = context;
            _manager = manager;
            _accessor = accessor;
            _logger = logger;
        }

        private async Task EnsureProblemExistsAsync(int id)
        {
            if (!await _context.Problems.AnyAsync(p => p.Id == id))
            {
                throw new NotFoundException();
            }
        }

        private async Task EnsureUserCanViewProblemAsync(int id)
        {
            var user = await _manager.GetUserAsync(_accessor.HttpContext.User);
            if (await _manager.IsInRoleAsync(user, ApplicationRoles.Administrator) ||
                await _manager.IsInRoleAsync(user, ApplicationRoles.ContestManager))
            {
                return;
            }

            var problem = await _context.Problems.FindAsync(id);
            await _context.Entry(problem).Reference<Contest>(p => p.Contest).LoadAsync();
            if (problem.Contest.IsPublic)
            {
                if (DateTime.Now.ToUniversalTime() < problem.Contest.BeginTime)
                {
                    throw new UnauthorizedAccessException("Not authorized to view this problem.");
                }
            }
            else
            {
                var registered = await _context.Registrations
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
            var userId = _accessor.HttpContext.User.GetSubjectId();
            var problems = await _context.Problems.PaginateAsync(pageIndex ?? 1, PageSize);
            var infos = new List<ProblemInfoDto>();
            foreach (var problem in problems.Items)
            {
                var solved = await _context.Submissions
                    .AnyAsync(s => s.ProblemId == problem.Id && s.UserId == userId && s.FailedOn == -1);
                infos.Add(new ProblemInfoDto(problem, solved));
            }

            return new PaginatedList<ProblemInfoDto>(problems.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<ProblemViewDto> GetProblemViewAsync(int id)
        {
            await EnsureProblemExistsAsync(id);
            await EnsureUserCanViewProblemAsync(id);
            return new ProblemViewDto(await _context.Problems.FindAsync(id));
        }
    }
}
