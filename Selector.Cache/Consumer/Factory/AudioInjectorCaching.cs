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
    public class CachingAudioFeatureInjectorFactory : IAudioFeatureInjectorFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IDatabaseAsync Db;

        public CachingAudioFeatureInjectorFactory(
            ILoggerFactory loggerFactory,
            IDatabaseAsync db
        )
        {
            LoggerFactory = loggerFactory;
            Db = db;
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
                    Db,
                    client.Tracks,
                    LoggerFactory.CreateLogger<CachingAudioFeatureInjector>()
                );
            }
            else
            {
                return new DummyAudioFeatureInjector(
                    watcher,
                    LoggerFactory.CreateLogger<DummyAudioFeatureInjector>()
                );
            }
        }
    }
}