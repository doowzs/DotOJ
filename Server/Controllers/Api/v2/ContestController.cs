﻿using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Exceptions;
using Server.Services;

namespace Server.Controllers.Api.v2
{
    [Authorize]
    [ApiController]
    [Route("api/v2/contest")]
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
        [HttpGet("current")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ContestInfoDto>>> ListCurrentContests()
        {
            return Ok(await _service.GetCurrentContestInfosAsync());
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ContestInfoDto>>> ListContests(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedContestInfosAsync(pageIndex));
        }

        [HttpGet("{id:int}")]
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

        [HttpGet("{id:int}/registrations")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContestViewDto>> ViewRegistrations(int id)
        {
            try
            {
                return Ok(await _service.GetRegistrationInfosAsync(id));
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
       
        [HttpGet("{id:int}/reviewDownload")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ContestViewDto>> ViewReviewList(int id)
        {
            try
            {
                return Ok(await _service.GetReviewListAsync(id));
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