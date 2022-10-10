using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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
            
        }
    }
}
