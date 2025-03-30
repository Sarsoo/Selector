using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Selector.Cache
{
    public interface ICacheWriterFactory
    {
        public Task<ISpotifyPlayerConsumer> Get(ISpotifyPlayerWatcher watcher = null);
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

        public Task<ISpotifyPlayerConsumer> Get(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult<ISpotifyPlayerConsumer>(new CacheWriter(
                watcher,
                Cache,
                LoggerFactory.CreateLogger<CacheWriter>()
            ));
        }
    }
}