using System;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;

namespace Selector
{
    public interface IPlayCounterFactory
    {
        public Task<ISpotifyPlayerConsumer> Get(LastfmClient fmClient = null, LastFmCredentials creds = null,
            ISpotifyPlayerWatcher watcher = null);
    }

    public class PlayCounterFactory : IPlayCounterFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly LastfmClient Client;
        private readonly LastFmCredentials Creds;

        public PlayCounterFactory(ILoggerFactory loggerFactory, LastfmClient client = null,
            LastFmCredentials creds = null)
        {
            LoggerFactory = loggerFactory;
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

            return Task.FromResult<ISpotifyPlayerConsumer>(new PlayCounter(
                watcher,
                client.Track,
                client.Album,
                client.Artist,
                client.User,
                credentials: creds ?? Creds,
                LoggerFactory.CreateLogger<PlayCounter>()
            ));
        }
    }
}