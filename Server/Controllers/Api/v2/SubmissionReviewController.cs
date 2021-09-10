using System;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.DTOs;
using Shared.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Exceptions;
using Server.Services;
using System.ComponentModel.DataAnnotations;


namespace Server.Controllers.Api.v2
{
    [Authorize]
    [ApiController]
    [Route("api/v2/submissionReview")]
    public class SubmissionReviewController : ControllerBase
    {
        private readonly ISubmissionReviewService _service;
        private readonly ILogger<SubmissionReviewController> _logger;

        public SubmissionReviewController(ISubmissionReviewService service, ILogger<SubmissionReviewController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<SubmissionInfoDto>>> ListSubmissionsReview(int problemId)
        {
            try
            {
                return Ok(await _service.GetSubmissionsToReviewListAsync(problemId));
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}