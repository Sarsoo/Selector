using Microsoft.Extensions.Logging;
using Selector.Spotify.ConfigFactory;
using SpotifyAPI.Web;

namespace Selector.Spotify.Consumer.Factory
{
    [Obsolete]
    public interface IAudioFeatureInjectorFactory
    {
        public Task<ISpotifyPlayerConsumer> Get(ISpotifyConfigFactory spotifyFactory,
            ISpotifyPlayerWatcher watcher = null);
    }

    [Obsolete]
    public class AudioFeatureInjectorFactory : IAudioFeatureInjectorFactory
    {
        private readonly ILoggerFactory LoggerFactory;

        public AudioFeatureInjectorFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public async Task<ISpotifyPlayerConsumer> Get(ISpotifyConfigFactory spotifyFactory,
            ISpotifyPlayerWatcher watcher = null)
        {
            if (!Magic.Dummy)
            {
                var config = await spotifyFactory.GetConfig();
                var client = new SpotifyClient(config);

                return new AudioFeatureInjector(
                    watcher,
                    client.Tracks,
                    LoggerFactory.CreateLogger<AudioFeatureInjector>()
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