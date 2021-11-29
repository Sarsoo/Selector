using Microsoft.Extensions.DependencyInjection;
using Selector.Web.Service;

namespace Selector.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCacheHubProxy(this IServiceCollection services)
        {
            services.AddSingleton<CacheHubProxy>();
            services.AddHostedService<CacheHubProxyService>();

            services.AddTransient<INowPlayingMappingFactory, NowPlayingMappingFactory>();
            services.AddScoped<IUserMapping, NowPlayingUserMapping>();
        }
    }
}
