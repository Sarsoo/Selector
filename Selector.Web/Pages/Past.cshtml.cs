using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Selector.Web.Extensions;

namespace Selector.Web.Pages
{
    public class PastModel : PageModel
    {
        private readonly ILogger<PastModel> Logger;

        public PastModel(ILogger<PastModel> logger)
        {
            Logger = logger;
        }

        public void OnGet()
        {
            Activity.Current?.Enrich(HttpContext);
        }
    }
}