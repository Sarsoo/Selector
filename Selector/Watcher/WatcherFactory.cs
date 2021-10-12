using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using SpotifyAPI.Web;

namespace Selector
{
    public class WatcherFactory : IWatcherFactory {

        private readonly ILoggerFactory LoggerFactory;
        private readonly IEqual Equal;

        public WatcherFactory(ILoggerFactory loggerFactory, IEqual equal)
        {
            LoggerFactory = loggerFactory;
            Equal = equal;
        }

        public async Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, int pollPeriod = 3000)
            where T : class, IWatcher
        {
            if(typeof(T).IsAssignableFrom(typeof(PlayerWatcher)))
            {
                var config = await spotifyFactory.GetConfig();
                var client = new SpotifyClient(config);

                return new PlayerWatcher(
                    client.Player, 
                    Equal, 
                    LoggerFactory?.CreateLogger<PlayerWatcher>(), 
                    pollPeriod: pollPeriod
                );
            }
            //else if (typeof(T).IsAssignableFrom(typeof(PlaylistWatcher)))
            //{

            //}
            else
            {
                throw new ArgumentException("Type unsupported");
            }
        }
    }
}
