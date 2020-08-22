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
    public interface IAssignmentService
    {
        public Task ValidateAssignmentId(int id);
        public Task ValidateAssignmentEditDto(AssignmentEditDto dto);
        public Task<PaginatedList<AssignmentInfoDto>> GetPaginatedAssignmentInfosAsync(int? pageIndex, string userId);
        public Task<AssignmentViewDto> GetAssignmentViewAsync(int id);
        public Task<AssignmentEditDto> GetAssignmentEditAsync(int id);
        public Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto);
        public Task<AssignmentEditDto> UpdateAssignmentAsync(int id, AssignmentEditDto dto);
        public Task DeleteAssignmentAsync(int id);
        public Task<PaginatedList<AssignmentRegistrationDto>> GetPaginatedRegistrationsAsync(int id, int? pageIndex);
        public Task RegisterUserForAssignmentAsync(int id, ApplicationUser user);
        public Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user);
    }

    public class AssignmentService : IAssignmentService
    {
        private const int PageSize = 20;
        private const int RegistrationPageSize = 50;

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

        public Task ValidateAssignmentEditDto(AssignmentEditDto dto)
        {
            if (string.IsNullOrEmpty(dto.Title))
            {
                throw new ValidationException("Title cannot be empty.");
            }

            if (!Enum.IsDefined(typeof(AssignmentMode), dto.Mode.GetValueOrDefault()))
            {
                throw new ValidationException("Invalid assignment mode.");
            }

            if (dto.BeginTime >= dto.EndTime)
            {
                throw new ValidationException("Invalid begin and end time.");
            }

            return Task.CompletedTask;
        }

        public async Task<PaginatedList<AssignmentInfoDto>>
            GetPaginatedAssignmentInfosAsync(int? pageIndex, string userId)
        {
            // See https://github.com/dotnet/efcore/issues/17068 for GroupJoin issues.
            var assignments = await _context.Assignments
                .OrderByDescending(a => a.Id)
                .PaginateAsync(pageIndex ?? 1, PageSize);
            IList<AssignmentInfoDto> infos;
            if (userId != null)
            {
                infos = new List<AssignmentInfoDto>();
                foreach (var assignment in assignments.Items)
                {
                    var registered = await _context.AssignmentRegistrations
                        .AnyAsync(r => r.AssignmentId == assignment.Id && r.UserId == userId);
                    infos.Add(new AssignmentInfoDto(assignment, registered));
                }
            }
            else
            {
                infos = assignments.Items.Select(a => new AssignmentInfoDto(a, false)).ToList();
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

        public async Task<AssignmentEditDto> GetAssignmentEditAsync(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment is null)
            {
                throw new NotFoundException();
            }

            return new AssignmentEditDto(assignment);
        }

        public async Task<AssignmentEditDto> CreateAssignmentAsync(AssignmentEditDto dto)
        {
            ValidateAssignmentEditDto(dto);
            var assignment = new Assignment()
            {
                Title = dto.Title,
                Description = dto.Description,
                IsPublic = dto.IsPublic.GetValueOrDefault(),
                Mode = dto.Mode.GetValueOrDefault(),
                BeginTime = dto.BeginTime,
                EndTime = dto.EndTime
            };
            await _context.Assignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
            return new AssignmentEditDto(assignment);
        }

        public async Task<AssignmentEditDto> UpdateAssignmentAsync(int id, AssignmentEditDto dto)
        {
            await ValidateAssignmentId(id);
            await ValidateAssignmentEditDto(dto);
            var assignment = await _context.Assignments.FindAsync(id);
            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.IsPublic = dto.IsPublic.GetValueOrDefault();
            assignment.Mode = dto.Mode.GetValueOrDefault();
            assignment.BeginTime = dto.BeginTime;
            assignment.EndTime = dto.EndTime;
            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();
            return new AssignmentEditDto(assignment);
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            await ValidateAssignmentId(id);
            var assignment = new Assignment {Id = id};
            _context.Assignments.Attach(assignment);
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedList<AssignmentRegistrationDto>>
            GetPaginatedRegistrationsAsync(int id, int? pageIndex)
        {
            return await _context.AssignmentRegistrations
                .Where(ar => ar.AssignmentId == id)
                .PaginateAsync(ar => new AssignmentRegistrationDto(ar), pageIndex ?? 1, RegistrationPageSize);
        }

        public async Task RegisterUserForAssignmentAsync(int id, ApplicationUser user)
        {
            var registered =
                await _context.AssignmentRegistrations.AnyAsync(r => r.AssignmentId == id && r.UserId == user.Id);
            if (!registered)
            {
                var registration = new AssignmentRegistration
                {
                    AssignmentId = id,
                    UserId = user.Id,
                    IsAssignmentManager = false,
                    IsParticipant = false,
                    Statistics = new AssignmentParticipantStatistics()
                };
                await _context.AssignmentRegistrations.AddAsync(registration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnregisterUserFromAssignmentAsync(int id, ApplicationUser user)
        {
            var registered =
                await _context.AssignmentRegistrations.AnyAsync(r => r.AssignmentId == id && r.UserId == user.Id);
            if (registered)
            {
                var registration = new AssignmentRegistration
                {
                    AssignmentId = id,
                    UserId = user.Id,
                };
                _context.AssignmentRegistrations.Attach(registration);
                _context.AssignmentRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
            }
        }
    }
}