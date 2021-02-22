using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Data.DTOs;
using Data.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Exceptions;
using WebApp.Services.Admin;

namespace WebApp.Controllers.Api.v1.Admin
{
    [Authorize(Policy = "ManageProblems")]
    [ApiController]
    [Route("api/v1/admin/problem")]
    public class AdminProblemController : ControllerBase
    {
        private readonly IAdminProblemService _service;
        private readonly ILogger<AdminProblemController> _logger;

        public AdminProblemController(IAdminProblemService service, ILogger<AdminProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ProblemInfoDto>>> ListProblems(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedProblemInfosAsync(pageIndex));
        }

        [HttpGet("{id:int}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaginatedList<ProblemEditDto>>> ViewProblem(int id)
        {
            try
            {
                return Ok(await _service.GetProblemEditAsync(id));
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
        public async Task<ActionResult<ProblemEditDto>> CreateProblem(ProblemEditDto dto)
        {
            try
            {
                return Created(nameof(ViewProblem), await _service.CreateProblemAsync(dto));
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProblemEditDto>> UpdateProblem(int id, ProblemEditDto dto)
        {
            try
            {
                return Ok(await _service.UpdateProblemAsync(id, dto));
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
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProblem(int id)
        {
            try
            {
                await _service.DeleteProblemAsync(id);
                return NoContent();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("{id:int}/test-cases")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ViewProblemTestCases(int id)
        {
            try
            {
                return Ok(await _service.GetProblemTestCasesAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id:int}/test-cases")]
        [RequestSizeLimit(300 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 300 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProblemTestCases(int id, [FromForm(Name = "zip-file")] IFormFile file)
        {
            try
            {
                return Ok(await _service.UpdateProblemTestCasesAsync(id, file));
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

        [HttpPost("import")]
        [RequestSizeLimit(300 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 300 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProblemEditDto>> ImportProblem
        ([Required, FromForm(Name = "contest-id")]
            int? contestId, [FromForm(Name = "zip-file")] IFormFile file)
        {
            try
            {
                return Ok(await _service.ImportProblemAsync(contestId.GetValueOrDefault(), file));
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

        [HttpGet("{id:int}/export")]
        [Produces(MediaTypeNames.Application.Zip)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExportProblem(int id)
        {
            try
            {
                var bytes = await _service.ExportProblemAsync(id);
                return File(bytes, MediaTypeNames.Application.Zip, id + ".zip");
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("{id:int}/export/submissions")]
        [Produces(MediaTypeNames.Application.Zip)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExportProblemSubmissions(int id, bool all)
        {
            try
            {
                var bytes = await _service.ExportProblemSubmissionsAsync(id, all);
                return File(bytes, MediaTypeNames.Application.Zip, id + "-submissions.zip");
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpGet("{id:int}/plagiarisms")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetProblemPlagiarismInfos(int id)
        {
            try
            {
                return Ok(await _service.GetProblemPlagiarismInfosAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id:int}/plagiarisms/check")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CheckProblemPlagiarism(int id)
        {
            try
            {
                return Ok(await _service.CheckProblemPlagiarismAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}