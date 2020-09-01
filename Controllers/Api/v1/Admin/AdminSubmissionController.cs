using Judge1.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Judge1.Controllers.Api.v1.Admin
{
    [Authorize(Policy = "ManageSubmissions")]
    [ApiController]
    [Route("api/v1/admin/submission")]
    public class AdminSubmissionController : ControllerBase
    {
        private readonly IAdminSubmissionService _service;
        private readonly ILogger<AdminSubmissionController> _logger;

        public AdminSubmissionController(IAdminSubmissionService service, ILogger<AdminSubmissionController> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}