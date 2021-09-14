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
    [Route("api/v2/ReviewFeedBack")]
    public class ReviewFeedBackController : ControllerBase
    {
        private readonly IReviewFeedBackService _service;
        private readonly ILogger<ReviewFeedBackController> _logger;

        public ReviewFeedBackController(IReviewFeedBackService service, ILogger<ReviewFeedBackController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<SubmissionReviewInfoDto>>> ListReviewFeedBack(int problemId)
        {
            try
            {
                return Ok(await _service.GetReviewFeedBackListAsync(problemId));
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}