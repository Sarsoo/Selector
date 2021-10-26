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
    public partial class SpotifyModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SpotifyModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
            StatusMessage = "Spotify Linked";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnlinkAsync()
        {
            StatusMessage = "Spotify Unlinked";
            return RedirectToPage();
        }
    }
}
