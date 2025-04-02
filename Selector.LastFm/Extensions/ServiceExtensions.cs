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

        services.AddTransient<ITrackApi>(sp => sp.GetService<LastfmClient>().Track);
        services.AddTransient<IAlbumApi>(sp => sp.GetService<LastfmClient>().Album);
        services.AddTransient<IArtistApi>(sp => sp.GetService<LastfmClient>().Artist);

        services.AddTransient<IUserApi>(sp => sp.GetService<LastfmClient>().User);

        services.AddTransient<IChartApi>(sp => sp.GetService<LastfmClient>().Chart);
        services.AddTransient<ILibraryApi>(sp => sp.GetService<LastfmClient>().Library);
        services.AddTransient<ITagApi>(sp => sp.GetService<LastfmClient>().Tag);

        services.AddTransient<IScrobbler, MemoryScrobbler>();

        return services;
    }
}