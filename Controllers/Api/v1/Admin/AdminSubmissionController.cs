using System.ComponentModel.DataAnnotations;
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
    [Authorize(Policy = "ManageSubmissions")]
    [ApiController]
    [Route("api/v1/admin/submission")]
    public class AdminSubmissionController : ControllerBase
    {
        private readonly IAdminSubmissionService _service;
        private readonly ILogger<AdminSubmissionController> _logger;

        public AdminSubmissionController(IAdminSubmissionService service, ILogger<AdminSubmissionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<SubmissionInfoDto>>> ListSubmissions
            (int? contestId, int? problemId, string userId, Verdict? verdict, int? pageIndex)
        {
            return Ok(await _service.GetPaginatedSubmissionInfosAsync
                (contestId, problemId, userId, verdict, pageIndex));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionEditDto>> ViewSubmission(int id)
        {
            try
            {
                return Ok(await _service.GetSubmissionEditAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPut("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionEditDto>> UpdateSubmission(int id, SubmissionEditDto dto)
        {
            try
            {
                return Ok(await _service.UpdateSubmissionAsync(id, dto));
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


        [HttpPut("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteSubmission(int id)
        {
            try
            {
                await _service.DeleteSubmissionAsync(id);
                return NoContent();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}