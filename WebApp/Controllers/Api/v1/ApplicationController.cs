using System.Net.Mime;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApp.Controllers.Api.v1
{
    [ApiController]
    [Route("api/v1/application")]
    public class ApplicationController : ControllerBase
    {
        private readonly IOptions<ApplicationConfig> _config;

        public ApplicationController(IOptions<ApplicationConfig> config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("config")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApplicationConfigDto> GetConfig()
        {
            return Ok(new ApplicationConfigDto(_config.Value));
        }
    }
}