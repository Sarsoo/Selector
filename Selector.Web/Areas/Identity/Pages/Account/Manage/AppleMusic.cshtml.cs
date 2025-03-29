using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Selector.Model;
using Selector.Events;

namespace Selector.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class AppleMusicModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SpotifyModel> Logger;
        private readonly RootOptions Config;
        private readonly UserEventBus UserEvent;

        public AppleMusicModel(
            UserManager<ApplicationUser> userManager,
            ILogger<SpotifyModel> logger,
            IOptions<RootOptions> config,
            UserEventBus userEvent
            )
        {
            _userManager = userManager;
            Logger = logger;
            Config = config.Value;

            UserEvent = userEvent;
        }

        [BindProperty]
        public bool AppleIsLinked { get; set; }

        [BindProperty]
        public DateTime LastRefresh { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            AppleIsLinked = user.AppleMusicLinked;
            LastRefresh = user.AppleMusicLastRefresh;

            return Page();
        }
    }
}
