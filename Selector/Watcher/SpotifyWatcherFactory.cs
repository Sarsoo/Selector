using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SpotifyAPI.Web;

namespace Selector
{
    public class SpotifyWatcherFactory : ISpotifyWatcherFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IEqual Equal;

        public SpotifyWatcherFactory(ILoggerFactory loggerFactory, IEqual equal)
        {
            LoggerFactory = loggerFactory;
            Equal = equal;
        }

        public async Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, string id = null,
            int pollPeriod = 3000)
            where T : class, IWatcher
        {
            if (typeof(T).IsAssignableFrom(typeof(SpotifyPlayerWatcher)))
            {
                if (!Magic.Dummy)
                {
                    var config = await spotifyFactory.GetConfig();
                    var client = new SpotifyClient(config);

                    // TODO: catch spotify exceptions
                    var user = await client.UserProfile.Current();

                    return new SpotifyPlayerWatcher(
                        client.Player,
                        Equal,
                        LoggerFactory?.CreateLogger<SpotifyPlayerWatcher>() ??
                        NullLogger<SpotifyPlayerWatcher>.Instance,
                        pollPeriod: pollPeriod
                    )
                    {
                        SpotifyUsername = user.DisplayName,
                        Id = id
                    };
                }
                else
                {
                    return new DummySpotifyPlayerWatcher(
                        Equal,
                        LoggerFactory?.CreateLogger<DummySpotifyPlayerWatcher>() ??
                        NullLogger<DummySpotifyPlayerWatcher>.Instance,
                        pollPeriod: pollPeriod
                    )
                    {
                        SpotifyUsername = "dummy",
                        Id = id
                    };
                }
            }
            else if (typeof(T).IsAssignableFrom(typeof(PlaylistWatcher)))
            {
                var config = await spotifyFactory.GetConfig();
                var client = new SpotifyClient(config);

                // TODO: catch spotify exceptions
                var user = await client.UserProfile.Current();

                return new PlaylistWatcher(
                    new(),
                    client,
                    LoggerFactory?.CreateLogger<PlaylistWatcher>() ?? NullLogger<PlaylistWatcher>.Instance,
                    pollPeriod: pollPeriod
                )
                {
                    SpotifyUsername = user.DisplayName,
                    Id = id
                };
            }
            else
            {
                throw new ArgumentException("Type unsupported");
            }
        }
    }
}