using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Selector.Model;
using Selector.Model.Extensions;

namespace Selector.Web.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScrobbleRepository _scrobbleRepo;

        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, IScrobbleRepository scrobbleRepo)
        {
            _logger = logger;
            _userManager = userManager;
            _scrobbleRepo = scrobbleRepo;
        }

        [BindProperty]
        public int? DailyScrobbles { get; set; }

        public void OnGet()
        {
            if(User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                if(user.ScrobbleSavingEnabled())
                {
                    DailyScrobbles = _scrobbleRepo.CountToday(userId: user.Id);
                }
            }
        }
    }
}
