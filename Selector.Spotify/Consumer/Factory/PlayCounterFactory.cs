using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;

namespace Selector.Spotify.Consumer.Factory
{
    public interface IPlayCounterFactory
    {
        public Task<ISpotifyPlayerConsumer> Get(LastfmClient? fmClient = null, LastFmCredentials? creds = null,
            ISpotifyPlayerWatcher? watcher = null);
    }

    public class PlayCounterFactory : IPlayCounterFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LastfmClient? _client;
        private readonly LastFmCredentials? _creds;

        public PlayCounterFactory(ILoggerFactory loggerFactory, LastfmClient? client = null,
            LastFmCredentials? creds = null)
        {
            _loggerFactory = loggerFactory;
            _client = client;
            _creds = creds;
        }

        public Task<ISpotifyPlayerConsumer> Get(LastfmClient? fmClient = null, LastFmCredentials? creds = null,
            ISpotifyPlayerWatcher? watcher = null)
        {
            var client = fmClient ?? _client;

            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return Task.FromResult<ISpotifyPlayerConsumer>(new PlayCounter(
                watcher,
                client.Track,
                client.Album,
                client.Artist,
                client.User,
                credentials: creds ?? _creds,
                _loggerFactory.CreateLogger<PlayCounter>()
            ));
        }
    }
}