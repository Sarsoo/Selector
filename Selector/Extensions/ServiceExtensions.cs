using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using IF.Lastfm.Core.Api;

namespace Selector.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddConsumerFactories(this IServiceCollection services)
        {
            services.AddTransient<IAudioFeatureInjectorFactory, AudioFeatureInjectorFactory>();
            services.AddTransient<AudioFeatureInjectorFactory>();

            services.AddTransient<IPlayCounterFactory, PlayCounterFactory>();
            services.AddTransient<PlayCounterFactory>();

            services.AddTransient<IWebHookFactory, WebHookFactory>();
            services.AddTransient<WebHookFactory>();

            return services;
        }

        public static IServiceCollection AddSpotify(this IServiceCollection services)
        {
            services.AddSingleton<IRefreshTokenFactoryProvider, RefreshTokenFactoryProvider>();
            services.AddSingleton<IRefreshTokenFactoryProvider, CachingRefreshTokenFactoryProvider>();

            return services;
        }

        public static IServiceCollection AddLastFm(this IServiceCollection services, string client, string secret)
        {
            var lastAuth = new LastAuth(client, secret);
            services.AddSingleton(lastAuth);
            services.AddTransient(sp => new LastfmClient(sp.GetService<LastAuth>()));

            services.AddTransient<ITrackApi>(sp => sp.GetService<LastfmClient>().Track);
            services.AddTransient<IAlbumApi>(sp => sp.GetService<LastfmClient>().Album);
            services.AddTransient<IArtistApi>(sp => sp.GetService<LastfmClient>().Artist);

            services.AddTransient<IUserApi>(sp => sp.GetService<LastfmClient>().User);

            services.AddTransient<IChartApi>(sp => sp.GetService<LastfmClient>().Chart);
            services.AddTransient<ILibraryApi>(sp => sp.GetService<LastfmClient>().Library);
            services.AddTransient<ITagApi>(sp => sp.GetService<LastfmClient>().Tag);

            return services;
        }

        public static IServiceCollection AddWatcher(this IServiceCollection services)
        {
            services.AddSingleton<IWatcherFactory, WatcherFactory>();
            services.AddSingleton<IWatcherCollectionFactory, WatcherCollectionFactory>();

            return services;
        }
    }
}
