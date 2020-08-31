using System.Net.Mime;
using System.Threading.Tasks;
using Judge1.Exceptions;
using Judge1.Models;
using Judge1.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judge1.Controllers.Api.v1.Admin
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
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ApplicationUserInfoDto>>> ListUsers(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedUserInfosAsync(pageIndex));
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
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
    }
}