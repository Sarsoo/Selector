using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Spotify;
using Selector.Spotify.ConfigFactory;
using Selector.Spotify.Consumer;
using Selector.Spotify.Consumer.Factory;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    [Obsolete]
    public class CachingAudioFeatureInjectorFactory : IAudioFeatureInjectorFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDatabaseAsync _db;

        public CachingAudioFeatureInjectorFactory(
            ILoggerFactory loggerFactory,
            IDatabaseAsync db
        )
        {
            _loggerFactory = loggerFactory;
            _db = db;
        }

        public async Task<ISpotifyPlayerConsumer> Get(ISpotifyConfigFactory spotifyFactory,
            ISpotifyPlayerWatcher watcher = null)
        {
            if (!Magic.Dummy)
            {
                var config = await spotifyFactory.GetConfig();
                var client = new SpotifyClient(config);

                return new CachingAudioFeatureInjector(
                    watcher,
                    _db,
                    client.Tracks,
                    _loggerFactory.CreateLogger<CachingAudioFeatureInjector>()
                );
            }
            else
            {
                return new DummyAudioFeatureInjector(
                    watcher,
                    _loggerFactory.CreateLogger<DummyAudioFeatureInjector>()
                );
            }
        }
    }
}