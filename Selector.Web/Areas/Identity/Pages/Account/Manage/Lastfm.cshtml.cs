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

namespace Selector.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class LastFmModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LastFmModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Username")]
            public string Username { get; set; }
        }

        private Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                Username = user.LastFmUsername,
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

        public async Task<IActionResult> OnPostChangeUsernameAsync()
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

            if (Input.Username != user.LastFmUsername)
            {
                user.LastFmUsername = Input.Username?.Trim();
                await _userManager.UpdateAsync(user);

                StatusMessage = "Username changed";
                return RedirectToPage();
            }

            StatusMessage = "Username unchanged";
            return RedirectToPage();
        }
    }
}
