using System.Net.Mime;
using System.Threading.Tasks;
using Judge1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Judge1.Controllers.Api.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IOptions<ApplicationConfig> _config;

        public ApplicationController(IOptions<ApplicationConfig> config)
        {
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("config")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApplicationConfigDto> GetConfig()
        {
            return Ok(new ApplicationConfigDto(_config.Value));
        }
    }
}