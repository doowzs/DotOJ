using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers.Api.v1
{
    [ApiController]
    [Route("api/v1/bulletin")]
    public class BulletinController : ControllerBase
    {
        private readonly IBulletinService _service;

        public BulletinController(IBulletinService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BulletinInfoDto>>> GetBulletins()
        {
            return Ok(await _service.GetBulletinInfosAsync());
        }
    }
}