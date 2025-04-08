using Microsoft.Extensions.DependencyInjection;
using Selector.AppleMusic.Consumer.Factory;
using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppleMusic(this IServiceCollection services)
    {
        services.AddSingleton<AppleMusicApiProvider>()
            .AddTransient<IAppleMusicWatcherFactory, AppleMusicWatcherFactory>()
            .AddTransient<IAppleMusicScrobblerFactory, AppleMusicScrobblerFactory>();

        return services;
    }
}