using Microsoft.Extensions.Logging;
using Selector.Spotify;

namespace Selector.Extensions
{
    public static class LoggingExtensions
    {
        public static IDisposable GetListeningEventArgsScope(this ILogger logger, SpotifyListeningChangeEventArgs e) =>
            logger.BeginScope(new Dictionary<string, object>()
                { { TraceConst.SpotifyUsername, e.SpotifyUsername }, { TraceConst.UserId, e.Id } });
    }
}