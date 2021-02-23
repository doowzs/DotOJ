using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Data.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApp.Services.Singleton;

namespace WebApp.Controllers.Api.v1
{
    [ApiController]
    [Route("api/v1/application")]
    public class ApplicationController : ControllerBase
    {
        private readonly IOptions<ApplicationConfig> _config;
        private readonly QueueStatisticsService _queueStatisticsService;

        public ApplicationController(IServiceProvider provider)
        {
            _config = provider.GetRequiredService<IOptions<ApplicationConfig>>();
            _queueStatisticsService = provider.GetRequiredService<QueueStatisticsService>();
        }

        [AllowAnonymous]
        [HttpGet("config")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApplicationConfigDto> GetConfig()
        {
            return Ok(new ApplicationConfigDto(_config.Value));
        }

        [HttpGet("statistics/average-queue-waiting-time")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetAverageQueueWaitingTime()
        {
            return Ok(await _queueStatisticsService.GetAverageWaitingSecondsAsync());
        }
    }
}