using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{    
    public class CachingAudioFeatureInjectorFactory: IAudioFeatureInjectorFactory {

        private readonly ILoggerFactory LoggerFactory;
        private readonly IDatabaseAsync Db;

        public CachingAudioFeatureInjectorFactory(
            ILoggerFactory loggerFactory,
            IDatabaseAsync db
        ) {
            LoggerFactory = loggerFactory;
            Db = db;
        }

        public async Task<IConsumer> Get(ISpotifyConfigFactory spotifyFactory, IPlayerWatcher watcher = null)
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
    }
}
