using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Shared.DTOs;
using Shared.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Exceptions;
using WebApp.Services.Admin;

namespace WebApp.Controllers.Api.v1.Admin
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/v1/admin/bulletin")]
    public class AdminBulletinController : ControllerBase
    {
        private readonly IAdminBulletinService _service;
        private readonly ILogger<AdminBulletinController> _logger;

        public AdminBulletinController(IAdminBulletinService service, ILogger<AdminBulletinController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<BulletinInfoDto>>> ListBulletins(int? pageIndex)
        {
            return Ok(await _service.GetPaginatedBulletinInfosAsync(pageIndex));
        }

        [HttpGet("{id:int}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BulletinEditDto>> ViewBulletin(int id)
        {
            try
            {
                return Ok(await _service.GetBulletinEditAsync(id));
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BulletinEditDto>> CreateBulletin(BulletinEditDto dto)
        {
            try
            {
                return Created(nameof(ViewBulletin), await _service.CreateBulletinAsync(dto));
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id:int}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BulletinEditDto>> UpdateBulletin(int id, BulletinEditDto dto)
        {
            try
            {
                return Ok(await _service.UpdateBulletinAsync(id, dto));
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

        [HttpDelete("{id:int}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteBulletin(int id)
        {
            try
            {
                await _service.DeleteBulletinAsync(id);
                return NoContent();
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}