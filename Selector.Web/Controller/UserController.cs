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
    public class UsersController : BaseAuthController
    {
        public UsersController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger
        ) : base(context, auth, userManager, logger)
        {
        }

        [HttpGet]
        [Authorize(Roles = Constants.AdminRole)]
        public async Task<ActionResult<IEnumerable<ApplicationUserDTO>>> Get()
        {
            Activity.Current?.Enrich(HttpContext);

            // TODO: Authorise
            return await Context.Users.AsNoTracking().Select(u => (ApplicationUserDTO)u).ToListAsync();
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseAuthController
    {
        public UserController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<UserController> logger
        ) : base(context, auth, userManager, logger)
        {
        }

        [HttpGet]
        public async Task<ActionResult<ApplicationUserDTO>> Get()
        {
            Activity.Current?.Enrich(HttpContext);

            var userId = UserManager.GetUserId(User);
            var user = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                Logger.LogWarning("No user found for [{id}], even though the 'me' route was used", userId);
                return NotFound();
            }

            var isAuthed = await AuthorizationService.AuthorizeAsync(User, user, UserOperations.Read);

            if (!isAuthed.Succeeded)
            {
                Logger.LogWarning("User [{username}] not authorised to view themselves?", user.UserName);
                return Unauthorized();
            }

            return (ApplicationUserDTO)user;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUserDTO>> GetById(string id)
        {
            Activity.Current?.Enrich(HttpContext);

            var usernameUpper = id.ToUpperInvariant();

            var user = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id)
                       ?? await Context.Users.AsNoTracking()
                           .FirstOrDefaultAsync(u => u.NormalizedUserName == usernameUpper);

            if (user is null)
            {
                return NotFound();
            }

            var isAuthed = await AuthorizationService.AuthorizeAsync(User, user, UserOperations.Read);

            if (!isAuthed.Succeeded)
            {
                return Unauthorized();
            }

            return (ApplicationUserDTO)user;
        }
    }
}