using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Judge1.Data;
using Judge1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Judge1.Services
{
    public interface IAssignmentService
    {
        public Task ValidateAssignmentId(int id);
        public Task ValidateAssignmentEditDto(AssignmentEditDto dto);
        public Task<AssignmentViewDto> GetAssignmentViewAsync(int id, bool privileged);
        public Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex, bool privileged);
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

        public async Task<AssignmentViewDto> GetAssignmentViewAsync(int id, bool privileged)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (!(privileged || DateTime.Now >= assignment.BeginTime))
            {
                throw new UnauthorizedAccessException("Not authorized to view this assignment.");
            }
            await _context.Entry(assignment).Collection(a => a.Registrations).LoadAsync();
            return new AssignmentViewDto(assignment);
        }

        public async Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex, bool privileged)
        {
            IQueryable<Assignment> data = _context.Assignments;
            if (!privileged)
            {
                data = data.Where(a => DateTime.Now >= a.BeginTime);
            }
            return await data.Include(a => a.Registrations)
                .PaginateAsync(a => new AssignmentInfoDto(a), pageIndex ?? 1, PageSize);
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