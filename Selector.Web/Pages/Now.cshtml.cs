using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Selector.Web.Extensions;

namespace Selector.Web.Pages
{
    public class NowModel : PageModel
    {
        private readonly ILogger<NowModel> Logger;

        public NowModel(ILogger<NowModel> logger)
        {
            Logger = logger;
        }

        public void OnGet()
        {
            Activity.Current?.Enrich(HttpContext);
        }
    }
}