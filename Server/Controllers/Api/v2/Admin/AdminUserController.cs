﻿using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Exceptions;
using Server.Services.Admin;

namespace Server.Controllers.Api.v2.Admin
{
    [Authorize(Policy = "ManageUsers")]
    [ApiController]
    [Route("api/v2/admin/user")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _service;
        private readonly ILogger<AdminProblemController> _logger;

        public AdminUserController(IAdminUserService service, ILogger<AdminProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<ApplicationUserInfoDto>>> ListUsers(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedUserInfosAsync(pageIndex));
        }

        [HttpGet("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> GetUser(string id)
        {
            try
            {
                return Ok(await _service.GetUserEditAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> UpdateUser(string id, ApplicationUserEditDto dto)
        {
            try
            {
                return Ok(await _service.UpdateUserAsync(id, dto));
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
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> ImportUsers()
        {
            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                return Ok(await _service.ImportUsersAsync(await reader.ReadToEndAsync()));
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApplicationUserEditDto>> DeleteUser(string id)
        {
            try
            {
                if (User.Identity?.Name == id)
                {
                    throw new ValidationException("Cannot delete yourself.");
                }

                await _service.DeleteUserAsync(id);
                return NoContent();
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
    }
}