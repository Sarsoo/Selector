using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Selector.Model.Authorisation
{
    public class UserIsSelfAuthHandler
        : AuthorizationHandler<OperationAuthorizationRequirement, ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserIsSelfAuthHandler(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            OperationAuthorizationRequirement requirement, 
            ApplicationUser resource
        ) {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }

            if (requirement.Name != Constants.ReadOpName &&
                requirement.Name != Constants.UpdateOpName &&
                requirement.Name != Constants.DeleteOpName)
            {
                return Task.CompletedTask;
            }

            if (resource.Id == userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
