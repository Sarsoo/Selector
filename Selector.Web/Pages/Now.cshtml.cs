using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;

using Selector.Web.Hubs;
using Selector.Web.Service;
using Selector.Model;

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
            
        }
    }
}
