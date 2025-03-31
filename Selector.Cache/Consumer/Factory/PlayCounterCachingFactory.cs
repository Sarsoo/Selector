using System;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using Selector.Spotify.Consumer.Factory;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class PlayCounterCachingFactory : IPlayCounterFactory
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

        public Task<ISpotifyPlayerConsumer> Get(LastfmClient fmClient = null, LastFmCredentials creds = null,
            ISpotifyPlayerWatcher watcher = null)
        {
            var client = fmClient ?? Client;

            if (client is null)
            {
                throw new ArgumentNullException("No Last.fm client provided");
            }

            return Task.FromResult<ISpotifyPlayerConsumer>(new PlayCounterCaching(
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