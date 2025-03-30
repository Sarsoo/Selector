using Microsoft.Extensions.DependencyInjection;
using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppleMusic(this IServiceCollection services)
    {
        services.AddSingleton<AppleMusicApiProvider>()
            .AddTransient<IAppleMusicWatcherFactory, AppleMusicWatcherFactory>();

        return services;
    }
}