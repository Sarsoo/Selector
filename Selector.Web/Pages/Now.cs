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
        private readonly INowPlayingMappingFactory MappingFactory;
        private readonly CacheHubProxy HubProxy;
        private readonly UserManager<ApplicationUser> UserManager;

        public NowModel(ILogger<NowModel> logger, 
            INowPlayingMappingFactory mappingFactory, 
            CacheHubProxy hubProxy, 
            UserManager<ApplicationUser> userManager)
        {
            Logger = logger;
            MappingFactory = mappingFactory;
            HubProxy = hubProxy;
            UserManager = userManager;
        }

        public void OnGet()
        {
            HubProxy.FormMapping(MappingFactory.Get(UserManager.GetUserId(User)));
        }
    }
}
