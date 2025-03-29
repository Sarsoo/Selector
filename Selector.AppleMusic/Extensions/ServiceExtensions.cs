using Microsoft.Extensions.DependencyInjection;

namespace Selector.AppleMusic.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppleMusic(this IServiceCollection services)
    {
        services.AddSingleton<AppleMusicApiProvider>();

        return services;
    }
}