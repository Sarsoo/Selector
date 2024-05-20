using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Selector.Web.Controller
{

    [ApiController]
    [Route("health")]
    public class HealthController : Microsoft.AspNetCore.Mvc.Controller
    {
        public HealthController()
        {

        }

        [HttpGet]
        [AllowAnonymous]
        public Task<ActionResult> Health()
        {
            return Task.FromResult<ActionResult>(Ok());
        }
    }
}