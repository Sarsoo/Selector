using Microsoft.Extensions.DependencyInjection;
using Selector.Web.Service;
using Selector.Web.Hubs;

namespace Selector.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCacheHubProxy(this IServiceCollection services)
        {
            services.AddScoped<EventHubProxy>();
            services.AddHostedService<CacheEventProxyService>();

            services.AddTransient<ICacheEventMapping, NowPlayingCacheMapping>();
            services.AddTransient<NowPlayingCacheMapping>();

            services.AddScoped<IEventHubMapping<NowPlayingHub, INowPlayingHubClient>, NowPlayingHubMapping>();
            services.AddScoped<NowPlayingHubMapping>();
        }
    }
}
