using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Selector.Cache
{
    public interface ICacheWriterFactory {
        public Task<IConsumer> Get(IPlayerWatcher watcher = null);
    }

    public class CacheWriterFactory: ICacheWriterFactory {

        private readonly ILoggerFactory LoggerFactory;
        private readonly IDatabaseAsync Cache;

        public CacheWriterFactory(
            IDatabaseAsync cache, 
            ILoggerFactory loggerFactory
        ) {
            Cache = cache;
            LoggerFactory = loggerFactory;
        }

        public Task<IConsumer> Get(IPlayerWatcher watcher = null)
        {
            return Task.FromResult<IConsumer>(new CacheWriter(
                watcher,
                Cache,
                LoggerFactory.CreateLogger<CacheWriter>()
            ));
        }
    }
}
