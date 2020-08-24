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
    public class ContestController : ControllerBase
    {
        private IContestService _service;
        private ILogger<ContestController> _logger;

        public ContestController(IContestService service, ILogger<ContestController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("upcoming")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ContestInfoDto>>> ListUpcomingContests()
        {
            var subject = User.Identity.IsAuthenticated ? User.GetSubjectId() : null;
            return Ok(await _service.GetUpcomingContestInfosAsync(subject));
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ContestInfoDto>>> ListContests(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedContestInfosAsync(pageIndex,
                User.Identity.IsAuthenticated ? User.GetSubjectId() : null));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContestViewDto>> ViewContest(int id)
        {
            try
            {
                return Ok(await _service.GetContestViewAsync(id));
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