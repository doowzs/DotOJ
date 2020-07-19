using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Judge1.Controllers.Api.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AssignmentController : ControllerBase
    {
    }
}