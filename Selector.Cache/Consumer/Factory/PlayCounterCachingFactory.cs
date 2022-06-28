using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;
using IF.Lastfm.Core.Api;

namespace Selector.Cache
{
    public class PlayCounterCachingFactory: IPlayCounterFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IDatabaseAsync Cache;
        private readonly LastfmClient Client;
        private readonly LastFmCredentials Creds;

        public PlayCounterCachingFactory(
            ILoggerFactory loggerFactory, 
            IDatabaseAsync cache,
            LastfmClient client = null, 
            LastFmCredentials creds = null)
        {
            LoggerFactory = loggerFactory;
            Cache = cache;
            Client = client;
            Creds = creds;
        }

        public Task<IPlayerConsumer> Get(LastfmClient fmClient = null, LastFmCredentials creds = null, IPlayerWatcher watcher = null)
        {
            var client = fmClient ?? Client;

            if (client is null)
            {
                throw new ArgumentNullException("No Last.fm client provided");
            }

            return Task.FromResult<IPlayerConsumer>(new PlayCounterCaching(
                watcher,
                client.Track,
                client.Album,
                client.Artist,
                client.User,
                Cache,
                credentials: creds ?? Creds,
                logger: LoggerFactory.CreateLogger<PlayCounterCaching>()
            ));
        }
    }
}
