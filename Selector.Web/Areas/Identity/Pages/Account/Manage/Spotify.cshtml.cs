using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Selector.Model;
using SpotifyAPI.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class SpotifyModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SpotifyModel> Logger;
        private readonly RootOptions Config;

        public SpotifyModel(
            UserManager<ApplicationUser> userManager,
            ILogger<SpotifyModel> logger,
            IOptions<RootOptions> config
            )
        {
            _userManager = userManager;
            Logger = logger;
            Config = config.Value;
        }

        [BindProperty]
        public bool SpotifyIsLinked { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            SpotifyIsLinked = user.SpotifyIsLinked;

            return Page();
        }

        public async Task<IActionResult> OnPostLinkAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if(Config.ClientId is null)
            {
                Logger.LogError($"Cannot link user, no Spotify client ID");
                StatusMessage = "Could not link Spotify, no app credentials";
                return RedirectToPage();
            }

            if (Config.ClientSecret is null)
            {
                Logger.LogError($"Cannot link user, no Spotify client secret");
                StatusMessage = "Could not link Spotify, no app credentials";
                return RedirectToPage();
            }

            var loginRequest = new LoginRequest(
                new Uri(Config.SpotifyCallback), 
                Config.ClientId, 
                LoginRequest.ResponseType.Code
            ) { 
                Scope = new[] { 
                    Scopes.UserReadPlaybackState
                } 
            };

            return Redirect(loginRequest.ToUri().ToString());
        }

        public async Task<IActionResult> OnPostUnlinkAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // TODO: stop users Spotify-linked resources (watchers)

            user.SpotifyIsLinked = false;

            user.SpotifyAccessToken = null;
            user.SpotifyRefreshToken = null;
            user.SpotifyTokenExpiry = 0;
            user.SpotifyLastRefresh = default;

            await _userManager.UpdateAsync(user);

            StatusMessage = "Spotify Unlinked";
            return RedirectToPage();
        }
    }
}
