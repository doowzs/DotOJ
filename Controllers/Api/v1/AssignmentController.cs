using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Judge1.Exceptions;
using Judge1.Models;
using Judge1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judge1.Controllers.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private IAssignmentService _service;
        private ILogger<AssignmentController> _logger;

        public AssignmentController(IAssignmentService service, ILogger<AssignmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("ongoing")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AssignmentInfoDto>>> ListPendingAssignments()
        {
            return Ok(await _service.GetOngoingAssignmentInfosAsync(User.GetSubjectId()));
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<AssignmentInfoDto>>> ListAssignments(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedAssignmentInfosAsync(pageIndex,
                User.Identity.IsAuthenticated ? User.GetSubjectId() : null));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssignmentViewDto>> ViewAssignment(int id)
        {
            try
            {
                return Ok(await _service.GetAssignmentViewAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
        }
    }
}