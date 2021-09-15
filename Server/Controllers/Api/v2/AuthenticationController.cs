using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Server.Services;
using Shared.DTOs;

namespace Server.Controllers.Api.v2
{
    [ApiController]
    [Route("/api/v2/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IServiceProvider provider)
        {
            _service = provider.GetRequiredService<IAuthenticationService>();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<String>> login(LoginRequestDto requestDto)
        {
            try
            {
                return Ok(await _service.Authenticate(requestDto.Username, requestDto.Password));
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("refresh")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<String>> refresh()
        {
            return Ok(await _service.Refresh());
        }
        
        [HttpPost("profile")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> ChangePassword(ChangePasswordRequestDto dto)
        {
            try
            {
                return Ok(await _service.ChangePassword(dto));
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}