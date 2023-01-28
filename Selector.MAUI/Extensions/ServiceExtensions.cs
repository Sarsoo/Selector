using System;
using Selector.MAUI.Services;
using Selector.SignalR;

namespace Selector.MAUI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddHubs(this IServiceCollection services)
    {
        services.AddSingleton<NowHubClient>()
                .AddSingleton<NowHubCache>();

        services.AddSingleton<PastHubClient>();

        services.AddSingleton<HubManager>();

        return services;
    }
}

