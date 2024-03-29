﻿using System;
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

        public async Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, string id = null, int pollPeriod = 3000)
            where T : class, IWatcher
        {
            if(typeof(T).IsAssignableFrom(typeof(PlayerWatcher)))
            {
                if(!Magic.Dummy)
                {
                    var config = await spotifyFactory.GetConfig();
                    var client = new SpotifyClient(config);

                    // TODO: catch spotify exceptions
                    var user = await client.UserProfile.Current();

                    return new PlayerWatcher(
                        client.Player,
                        Equal,
                        LoggerFactory?.CreateLogger<PlayerWatcher>() ?? NullLogger<PlayerWatcher>.Instance,
                        pollPeriod: pollPeriod
                    )
                    {
                        SpotifyUsername = user.DisplayName,
                        Id = id
                    };
                }
                else
                {
                    return new DummyPlayerWatcher(
                        Equal,
                        LoggerFactory?.CreateLogger<DummyPlayerWatcher>() ?? NullLogger<DummyPlayerWatcher>.Instance,
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
