using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Selector.Model;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Controller {
    
    public class BaseAuthController: Microsoft.AspNetCore.Mvc.Controller
    {
        protected ApplicationDbContext Context { get; }
        protected IAuthorizationService AuthorizationService { get; }
        protected UserManager<ApplicationUser> UserManager { get; }
        protected ILogger<BaseAuthController> Logger { get; }

        public BaseAuthController(
            ApplicationDbContext context,
            IAuthorizationService auth,
            UserManager<ApplicationUser> userManager,
            ILogger<BaseAuthController> logger
        ) {
            Context = context;
            AuthorizationService = auth;
            UserManager = userManager;
            Logger = logger;
        }
    }
}