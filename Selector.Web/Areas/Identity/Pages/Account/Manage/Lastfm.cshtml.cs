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
using Selector.Events;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class LastFmModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserEventBus UserEvent;

        private readonly ILogger<LastFmModel> logger;

        public LastFmModel(
            UserManager<ApplicationUser> userManager,
            UserEventBus userEvent,
            ILogger<LastFmModel> _logger)
        {
            _userManager = userManager;
            UserEvent = userEvent;

            logger = _logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Display(Name = "Scrobble Saving")]
            public bool ScrobbleSaving { get; set; }
        }

        private Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                Username = user.LastFmUsername,
                ScrobbleSaving = user.SaveScrobbles
            };

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var changed = false;

            if (Input.Username != user.LastFmUsername)
            {
                var oldUsername = user.LastFmUsername;
                user.LastFmUsername = Input.Username?.Trim();

                changed = true;

                UserEvent.OnLastfmCredChange(this, new LastfmChange { 
                    UserId = user.Id, 
                    PreviousUsername = oldUsername, 
                    NewUsername = user.LastFmUsername
                });

                logger.LogInformation("Changing username from {} to {}", oldUsername, user.LastFmUsername);

                StatusMessage = "Username changed";
            }

            if (Input.ScrobbleSaving != user.SaveScrobbles)
            {
                user.SaveScrobbles = Input.ScrobbleSaving;

                logger.LogInformation("Changing scrobble saving from {} to {}", !Input.ScrobbleSaving, Input.ScrobbleSaving);
                
                if (changed)
                {
                    StatusMessage += ", scrobble saving updated";
                }
                else
                {
                    StatusMessage = "Scrobble saving updated";
                    changed = true;
                }
            }

            if (changed)
            {
                logger.LogInformation("Saving Last.fm settings for {}", user.LastFmUsername);

                await _userManager.UpdateAsync(user);
            }
            else
            {
                StatusMessage = "Settings unchanged";
            }

            return RedirectToPage();
        }
    }
}
