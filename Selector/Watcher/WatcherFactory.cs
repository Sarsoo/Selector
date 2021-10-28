using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

                var user = await client.UserProfile.Current();

                return new PlayerWatcher(
                    client.Player, 
                    Equal, 
                    LoggerFactory?.CreateLogger<PlayerWatcher>() ?? NullLogger<PlayerWatcher>.Instance, 
                    pollPeriod: pollPeriod
                ) {
                    Username = user.DisplayName
                };
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
