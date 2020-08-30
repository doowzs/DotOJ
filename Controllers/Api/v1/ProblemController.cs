using System;
using System.Net.Mime;
using System.Threading.Tasks;
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
    [Route("api/v1/problem")]
    public class ProblemController : ControllerBase
    {
        private readonly IProblemService _service;
        private readonly ILogger<ProblemController> _logger;

        public ProblemController(IProblemService service, ILogger<ProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ProblemInfoDto>>> ListProblems(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedProblemInfosAsync(pageIndex));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProblemViewDto>> ViewProblem(int id)
        {
            try
            {
                return Ok(await _service.GetProblemViewAsync(id));
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