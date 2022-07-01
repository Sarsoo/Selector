using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Selector
{
    public static class LoggingExtensions
    {
        public static IDisposable GetListeningEventArgsScope(this ILogger logger, ListeningChangeEventArgs e) => logger.BeginScope(new Dictionary<string, object>() { { "spotify_username", e.SpotifyUsername }, { "id", e.Id } });
    }
}

