using System.ComponentModel.DataAnnotations;
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

namespace Judge1.Controllers.Api.v1.Admin
{
    [Authorize(Policy = "ManageAssignments")]
    [ApiController]
    [Route("api/v1/admin/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _service;
        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(IAssignmentService service, ILogger<AssignmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<AssignmentInfoDto>>> ListAssignments(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedAssignmentInfosAsync(pageIndex, null));
        }

        [HttpGet("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssignmentEditDto>> ViewAssignment(int id)
        {
            try
            {
                return Ok(await _service.GetAssignmentEditAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssignmentEditDto>> CreateAssignment(AssignmentEditDto dto)
        {
            try
            {
                var assignment = await _service.CreateAssignmentAsync(dto);
                return Created(nameof(ViewAssignment), assignment);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssignmentEditDto>> UpdateAssignment(int id, AssignmentEditDto dto)
        {
            try
            {
                var assignment = await _service.UpdateAssignmentAsync(id, dto);
                return Ok(assignment);
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

        [HttpDelete("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAssignment(int id)
        {
            try
            {
                await _service.DeleteAssignmentAsync(id);
                return NoContent();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}