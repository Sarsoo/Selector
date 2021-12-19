using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Selector.Events;
using Selector.Model.Authorisation;
using Selector.Model.Events;

namespace Selector.Model.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddModelEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>();
            services.AddSingleton<IEventBus, UserEventBus>(sp => sp.GetService<UserEventBus>());
        }

        public static void AddAuthorisationHandlers(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            services.AddScoped<IAuthorizationHandler, WatcherIsOwnerAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, WatcherIsAdminAuthHandler>();

            services.AddScoped<IAuthorizationHandler, UserIsSelfAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, UserIsAdminAuthHandler>();
        }
    }
}
