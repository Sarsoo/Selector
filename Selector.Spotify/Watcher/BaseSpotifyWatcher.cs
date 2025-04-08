using Microsoft.Extensions.Logging;

namespace Selector.Spotify.Watcher;

public abstract class BaseSpotifyWatcher(ILogger<BaseWatcher>? logger) : BaseWatcher(logger)
{
    public required string SpotifyUsername { get; set; }

    protected override Dictionary<string, object> LogScopeContext =>
        new[]
            {
                base.LogScopeContext,
                new Dictionary<string, object>() { { "spotify_username", SpotifyUsername } }
            }
            .SelectMany(x => x)
            .ToDictionary();
}