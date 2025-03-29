using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Selector.Events;
using Selector.Model;

namespace Selector.Web.Controller
{
    public class TokenPost
    {
        public string Key { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AppleMusicController : BaseAuthController
    {
        private readonly UserEventBus UserEvent;

        public AppleMusicController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger,
            UserEventBus userEvent
        ) : base(context, auth, userManager, logger) 
        {
            UserEvent = userEvent;
        }

        [HttpPost]
        [Route("token")]
        public async Task<ActionResult> Token(TokenPost request)
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ArgumentNullException("No user returned");
            }

            var alreadyAuthed = user.AppleMusicLinked;

            user.AppleMusicKey = request.Key;
            user.AppleMusicLinked = true;
            user.AppleMusicLastRefresh = DateTime.UtcNow;

            await UserManager.UpdateAsync(user);

            UserEvent.OnAppleMusicLinkChange(this, new AppleMusicLinkChange { UserId = user.Id, PreviousLinkState = alreadyAuthed, NewLinkState = true });

            return Ok();
        }

        [HttpPost]
        [Route("unlink")]
        public async Task<ActionResult> Unlink()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ArgumentNullException("No user returned");
            }

            var alreadyAuthed = user.AppleMusicLinked;

            user.AppleMusicKey = null;
            user.AppleMusicLinked = false;
            user.AppleMusicLastRefresh = DateTime.MinValue;

            await UserManager.UpdateAsync(user);

            UserEvent.OnAppleMusicLinkChange(this, new AppleMusicLinkChange { UserId = user.Id, PreviousLinkState = alreadyAuthed, NewLinkState = false });

            return Ok();
        }
    }
}