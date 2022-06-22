using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Selector.Cache;
using Selector.Model.Authorisation;

namespace Selector.Model.Extensions
{
    public static class ServiceExtensions
    {
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

        public static IServiceCollection AddDBPlayCountPuller(this IServiceCollection services)
        {
            services.AddTransient<DBPlayCountPuller>();

            return services;
        }
    }
}
