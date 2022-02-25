using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using IF.Lastfm.Core.Api;

namespace Selector
{
    public interface IPlayCounterFactory
    {
        public Task<IConsumer> Get(LastfmClient fmClient = null, LastFmCredentials creds = null, IPlayerWatcher watcher = null);
    }
    
    public class PlayCounterFactory: IPlayCounterFactory {

        private readonly ILoggerFactory LoggerFactory;
        private readonly LastfmClient Client;
        private readonly LastFmCredentials Creds;

        public PlayCounterFactory(ILoggerFactory loggerFactory, LastfmClient client = null, LastFmCredentials creds = null)
        {
            LoggerFactory = loggerFactory;
            Client = client;
            Creds = creds;
        }

        public Task<IConsumer> Get(LastfmClient fmClient = null, LastFmCredentials creds = null, IPlayerWatcher watcher = null)
        {
            var client = fmClient ?? Client;

            if(client is null)
            {
                throw new ArgumentNullException("No Last.fm client provided");
            }

            return Task.FromResult<IConsumer>(new PlayCounter(
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
