using Microsoft.Extensions.DependencyInjection;
using Selector.Spotify.Consumer.Factory;
using Selector.Spotify.FactoryProvider;

namespace Selector.Extensions;

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
}