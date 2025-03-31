using Microsoft.Extensions.DependencyInjection;

namespace Selector.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddWatcher(this IServiceCollection services)
        {
            services.AddSingleton<IWatcherCollectionFactory, WatcherCollectionFactory>();

            return services;
        }
    }
}