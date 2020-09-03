using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Data.Models;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Exceptions;
using WebApp.Services.Admin;

namespace WebApp.Controllers.Api.v1.Admin
{
    [Authorize(Policy = "ManageUsers")]
    [ApiController]
    [Route("api/v1/admin/user")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _service;
        private readonly ILogger<AdminProblemController> _logger;

        public AdminUserController(IAdminUserService service, ILogger<AdminProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ApplicationUserInfoDto>>> ListUsers(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedUserInfosAsync(pageIndex));
        }

        [HttpGet("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> GetUser(string id)
        {
            try
            {
                return Ok(await _service.GetUserEditAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> UpdateUser(string id, ApplicationUserEditDto dto)
        {
            try
            {
                return Ok(await _service.UpdateUserAsync(id, dto));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> DeleteUser(string id)
        {
            try
            {
                if (User.GetSubjectId() == id)
                {
                    throw new ValidationException("Cannot delete yourself.");
                }

                await _service.DeleteUserAsync(id);
                return NoContent();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}