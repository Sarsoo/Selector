using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.Model;
using Selector.Model.Authorisation;
using Selector.Web.Extensions;

namespace Selector.Web.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchersController : BaseAuthController
    {
        public WatchersController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<WatchersController> logger
        ) : base(context, auth, userManager, logger)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Watcher>>> Get()
        {
            Activity.Current?.Enrich(HttpContext);

            var isAuthed = User.IsInRole(Constants.AdminRole);

            if (isAuthed)
            {
                return await Context.Watcher.AsNoTracking().ToListAsync();
            }
            else
            {
                var userId = UserManager.GetUserId(User);
                return await Context.Watcher.AsNoTracking().Where(w => w.UserId == userId).ToListAsync();
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class WatcherController : BaseAuthController
    {
        public WatcherController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<WatcherController> logger
        ) : base(context, auth, userManager, logger)
        {
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Watcher>> Get(int id)
        {
            Activity.Current?.Enrich(HttpContext);

            var watcher = await Context.Watcher.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id);

            if (watcher is null)
            {
                return NotFound();
            }

            var isAuthed = await AuthorizationService.AuthorizeAsync(User, watcher, WatcherOperations.Read);

            if (!isAuthed.Succeeded)
            {
                return Unauthorized();
            }

            return watcher;
        }
    }
}