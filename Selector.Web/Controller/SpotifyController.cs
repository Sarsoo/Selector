using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selector.Events;
using Selector.Model;
using Selector.Web.Extensions;
using SpotifyAPI.Web;

namespace Selector.Web.Controller
{
    [ApiController]
    [Route("api/[controller]/callback")]
    public class SpotifyController : BaseAuthController
    {
        private readonly RootOptions Config;
        private readonly UserEventBus UserEvent;
        private const string ManageSpotifyPath = "/Identity/Account/Manage/Spotify";

        public SpotifyController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger,
            IOptions<RootOptions> config,
            UserEventBus userEvent
        ) : base(context, auth, userManager, logger)
        {
            Config = config.Value;
            UserEvent = userEvent;
        }

        [HttpGet]
        public async Task<RedirectResult> Callback(string code)
        {
            Activity.Current?.Enrich(HttpContext);

            if (Config.ClientId is null)
            {
                Logger.LogError("Cannot link user, no Spotify client ID");
                TempData["StatusMessage"] = "Could not link Spotify, no app credentials";
                return Redirect(ManageSpotifyPath);
            }

            if (Config.ClientSecret is null)
            {
                Logger.LogError("Cannot link user, no Spotify client secret");
                TempData["StatusMessage"] = "Could not link Spotify, no app credentials";
                return Redirect(ManageSpotifyPath);
            }

            var user = await UserManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ArgumentNullException("No user returned");
            }

            // TODO: Authorise
            var response = await new OAuthClient()
                .RequestToken(
                    new AuthorizationCodeTokenRequest(
                        Config.ClientId,
                        Config.ClientSecret,
                        code,
                        new Uri(Config.SpotifyCallback)
                    )
                );

            user.SpotifyIsLinked = true;

            user.SpotifyAccessToken = response.AccessToken;
            user.SpotifyRefreshToken = response.RefreshToken;
            user.SpotifyLastRefresh = response.CreatedAt;
            user.SpotifyTokenExpiry = response.ExpiresIn;

            await UserManager.UpdateAsync(user);

            UserEvent.OnSpotifyLinkChange(this,
                new SpotifyLinkChange { UserId = user.Id, PreviousLinkState = false, NewLinkState = true });

            TempData["StatusMessage"] = "Spotify Linked";
            return Redirect(ManageSpotifyPath);
        }
    }
}