using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic.Consumer;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public interface ICacheWriterFactory
    {
        public Task<ISpotifyPlayerConsumer> GetSpotify(ISpotifyPlayerWatcher watcher = null);
        public Task<IApplePlayerConsumer> GetApple(IAppleMusicPlayerWatcher watcher = null);
    }

    public class CacheWriterFactory : ICacheWriterFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IDatabaseAsync Cache;

        public CacheWriterFactory(
            IDatabaseAsync cache,
            ILoggerFactory loggerFactory
        )
        {
            Cache = cache;
            LoggerFactory = loggerFactory;
        }

        public Task<ISpotifyPlayerConsumer> GetSpotify(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult<ISpotifyPlayerConsumer>(new SpotifyCacheWriter(
                watcher,
                Cache,
                LoggerFactory.CreateLogger<SpotifyCacheWriter>()
            ));
        }

        public Task<IApplePlayerConsumer> GetApple(IAppleMusicPlayerWatcher watcher = null)
        {
            return Task.FromResult<IApplePlayerConsumer>(new AppleCacheWriter(
                watcher,
                Cache,
                LoggerFactory.CreateLogger<AppleCacheWriter>()
            ));
        }
    }
}