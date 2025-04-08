using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Scrobblers;
using Microsoft.Extensions.DependencyInjection;

namespace Selector.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddLastFm(this IServiceCollection services, string client, string secret)
    {
        services.AddTransient<ILastAuth>(sp => new LastAuth(client, secret));
        services.AddTransient<LastAuth>(sp => new LastAuth(client, secret));
        services.AddTransient(sp => new LastfmClient(sp.GetService<LastAuth>()));

        services.AddTransient<ITrackApi>(sp => sp.GetRequiredService<LastfmClient>().Track);
        services.AddTransient<IAlbumApi>(sp => sp.GetRequiredService<LastfmClient>().Album);
        services.AddTransient<IArtistApi>(sp => sp.GetRequiredService<LastfmClient>().Artist);

        services.AddTransient<IUserApi>(sp => sp.GetRequiredService<LastfmClient>().User);

        services.AddTransient<IChartApi>(sp => sp.GetRequiredService<LastfmClient>().Chart);
        services.AddTransient<ILibraryApi>(sp => sp.GetRequiredService<LastfmClient>().Library);
        services.AddTransient<ITagApi>(sp => sp.GetRequiredService<LastfmClient>().Tag);

        services.AddTransient<IScrobbler, MemoryScrobbler>();

        return services;
    }
}