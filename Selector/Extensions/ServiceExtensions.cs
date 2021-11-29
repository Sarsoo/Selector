using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using IF.Lastfm.Core.Api;

namespace Selector.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddConsumerFactories(this IServiceCollection services)
        {
            services.AddTransient<IAudioFeatureInjectorFactory, AudioFeatureInjectorFactory>();
            services.AddTransient<AudioFeatureInjectorFactory>();

            services.AddTransient<IPlayCounterFactory, PlayCounterFactory>();
            services.AddTransient<PlayCounterFactory>();
        }

        public static void AddSpotify(this IServiceCollection services)
        {
            services.AddSingleton<IRefreshTokenFactoryProvider, RefreshTokenFactoryProvider>();
            services.AddSingleton<IRefreshTokenFactoryProvider, CachingRefreshTokenFactoryProvider>();
        }

        public static void AddLastFm(this IServiceCollection services, string client, string secret)
        {
            var lastAuth = new LastAuth(client, secret);
            services.AddSingleton(lastAuth);
            services.AddTransient(sp => new LastfmClient(sp.GetService<LastAuth>()));
        }

        public static void AddWatcher(this IServiceCollection services)
        {
            services.AddSingleton<IWatcherFactory, WatcherFactory>();
            services.AddSingleton<IWatcherCollectionFactory, WatcherCollectionFactory>();
        }
    }
}
