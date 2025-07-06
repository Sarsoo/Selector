using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Selector.Web.Extensions;

namespace Selector.Web.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            Activity.Current?.Enrich(HttpContext);
        }
    }
}