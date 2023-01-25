using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using SpotifyAPI.Web;

namespace Selector
{
    public interface IAudioFeatureInjectorFactory
    {
        public Task<IPlayerConsumer> Get(ISpotifyConfigFactory spotifyFactory, IPlayerWatcher watcher = null);
    }
    
    public class AudioFeatureInjectorFactory: IAudioFeatureInjectorFactory {

        private readonly ILoggerFactory LoggerFactory;

        public AudioFeatureInjectorFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public async Task<IPlayerConsumer> Get(ISpotifyConfigFactory spotifyFactory, IPlayerWatcher watcher = null)
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
