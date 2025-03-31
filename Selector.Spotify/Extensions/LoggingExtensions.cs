using Microsoft.Extensions.Logging;
using Selector.Spotify;

namespace Selector.Extensions
{
    public static class LoggingExtensions
    {
        public static IDisposable GetListeningEventArgsScope(this ILogger logger, SpotifyListeningChangeEventArgs e) =>
            logger.BeginScope(new Dictionary<string, object>()
                { { "spotify_username", e.SpotifyUsername }, { "id", e.Id } });
    }
}