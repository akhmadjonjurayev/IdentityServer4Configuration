using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Identihaus.Controllers
{
    [Route("identity")]
    public class IdentityController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
        [HttpGet("auth")]
        public IActionResult CheckAuthorize()
        {
            return Ok("success");
        }
    }
}
