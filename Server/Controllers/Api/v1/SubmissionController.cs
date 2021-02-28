using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Exceptions;
using Server.Services;

namespace Server.Controllers.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v1/submission")]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _service;
        private readonly ILogger<SubmissionController> _logger;

        public SubmissionController(ISubmissionService service, ILogger<SubmissionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<SubmissionInfoDto>>>
            ListSubmissions(int? contestId, string userId, string contestantId,
                int? problemId, Verdict? verdict, int? pageSize, int? pageIndex)
        {
            return Ok(await _service.GetPaginatedSubmissionsAsync(contestId, userId,
                contestantId, problemId, verdict, pageSize, pageIndex));
        }

        [HttpGet("batch")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SubmissionInfoDto>>> GetBatchSubmissionInfos
            ([FromQuery(Name = "id")] List<int> ids)
        {
            return Ok(await _service.GetBatchSubmissionInfosAsync(ids));
        }

        [HttpGet("{id:int}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionInfoDto>> GetSubmissionInfo(int id)
        {
            try
            {
                var submission = await _service.GetSubmissionInfoAsync(id);
                return Ok(submission);
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

        [HttpGet("{id:int}/detail")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmissionViewDto>> GetSubmissionView(int id)
        {
            try
            {
                var submission = await _service.GetSubmissionViewAsync(id);
                return Ok(submission);
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

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<SubmissionInfoDto>> CreateSubmission(SubmissionCreateDto dto)
        {
            try
            {
                return Ok(await _service.CreateSubmissionAsync(dto));
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (TooManyRequestsException e)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, e.Message);
            }
        }

        [HttpGet("lab/token")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetTestKitLabSubmitToken(int problemId)
        {
            try
            {
                return await _service.GetTestKitLabSubmitTokenAsync(problemId);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("lab")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [Consumes("multipart/form-data")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<string>> CreateTestKitLabSubmission
            ([FromForm(Name = "TOKEN")] string token, [FromForm(Name = "FILE")] IFormFile file)
        {
            try
            {
                return await _service.CreateTestKitLabSubmissionAsync(token, file);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return Unauthorized(e.Message);
            }
            catch (TooManyRequestsException e)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, $"TooManyRequests: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"InternalServerError: {e.Message}");
            }
        }
    }
}