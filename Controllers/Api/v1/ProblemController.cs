using System.Threading.Tasks;
using Judge1.Models;
using Judge1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judge1.Controllers.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProblemController : ControllerBase
    {
        private readonly IProblemService _service;
        private readonly ILogger<ProblemController> _logger;

        public ProblemController(IProblemService service, ILogger<ProblemController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<PaginatedList<ProblemInfoDto>> GetProblems(int? pageIndex)
        {
            return await _service.GetPaginatedProblemInfosAsync(pageIndex);
        }
    }
}