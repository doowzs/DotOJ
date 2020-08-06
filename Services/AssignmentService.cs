using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Data;
using Judge1.Exceptions;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IAssignmentService
    {
        public Task ValidateAssignmentId(int id);
        public Task ValidateAssignmentEditDto(AssignmentEditDto dto);
        public Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex, string userId);
        public Task<AssignmentViewDto> GetAssignmentViewAsync(int id);
        public Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto);
        public Task<AssignmentEditDto> UpdateAssignmentAsync(AssignmentEditDto dto);
        public Task DeleteAssignmentAsync(int id);
        public Task RegisterUserForAssignmentAsync(int id, ApplicationUser user);
        public Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user);
    }

    public class AssignmentService : IAssignmentService
    {
        private const int PageSize = 20;
        
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(ApplicationDbContext context, ILogger<AssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ValidateAssignmentId(int id)
        {
            if (!await _context.Assignments.AnyAsync(a => a.Id == id))
            {
                throw new ValidationException("Invalid assignment ID.");
            }
        }

        public async Task ValidateAssignmentEditDto(AssignmentEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public async Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex, string userId)
        {
            // See https://github.com/dotnet/efcore/issues/17068 for GroupJoin issues.
            var assignments = await _context.Assignments
                .OrderByDescending(a => a.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            var infos = new List<AssignmentInfoDto>();
            foreach (var assignment in assignments.Items)
            {
                var registered = await _context.AssignmentRegistrations
                    .AnyAsync(r => r.AssignmentId == assignment.Id && r.UserId == userId);
                infos.Add(new AssignmentInfoDto(assignment, registered));
            }
            return new PaginatedList<AssignmentInfoDto>(assignments.TotalItems, pageIndex ?? 1, PageSize, infos);
        }

        public async Task<AssignmentViewDto> GetAssignmentViewAsync(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment is null)
            {
                throw new NotFoundException();
            }
            
            if (DateTime.Now < assignment.BeginTime)
            {
                throw new UnauthorizedAccessException("Not authorized to view this assignment.");
            }
            
            await _context.Entry(assignment).Collection(a => a.Problems).LoadAsync();
            await _context.Entry(assignment).Collection(a => a.Notices).LoadAsync();
            return new AssignmentViewDto(assignment);
        }

        public async Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public async Task<AssignmentEditDto> UpdateAssignmentAsync(AssignmentEditDto dto)
        {
            throw new System.NotImplementedException();
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task RegisterUserForAssignmentAsync(int id, ApplicationUser user)
        {
            throw new System.NotImplementedException();
        }

        public Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}