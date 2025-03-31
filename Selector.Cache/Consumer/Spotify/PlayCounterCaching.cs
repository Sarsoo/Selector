using System;
using System.Threading;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;
using Selector.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class PlayCounterCaching : PlayCounter
    {
        private readonly IDatabaseAsync Db;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromDays(1);

        public PlayCounterCaching(
            ISpotifyPlayerWatcher watcher,
            ITrackApi trackClient,
            IAlbumApi albumClient,
            IArtistApi artistClient,
            IUserApi userClient,
            IDatabaseAsync db,
            LastFmCredentials credentials = null,
            ILogger<PlayCounterCaching> logger = null,
            CancellationToken token = default
        ) : base(watcher, trackClient, albumClient, artistClient, userClient, credentials, logger, token)
        {
            Db = db;

            NewPlayCount += CacheCallback;
        }

        public void CacheCallback(object sender, PlayCount e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await AsyncCacheCallback(e);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public async Task AsyncCacheCallback(PlayCount e)
        {
            var track = e.SpotifyListeningEvent.Current.Item as FullTrack;
            Logger.LogTrace("Caching play count for [{track}]", track.DisplayString());

            var tasks = new Task[]
            {
                Db.StringSetAsync(Key.TrackPlayCount(e.Username, track.Name, track.Artists[0].Name), e.Track,
                    expiry: CacheExpiry),
                Db.StringSetAsync(Key.AlbumPlayCount(e.Username, track.Album.Name, track.Album.Artists[0].Name),
                    e.Album, expiry: CacheExpiry),
                Db.StringSetAsync(Key.ArtistPlayCount(e.Username, track.Artists[0].Name), e.Artist,
                    expiry: CacheExpiry),
                Db.StringSetAsync(Key.UserPlayCount(e.Username), e.User, expiry: CacheExpiry),
            };

            await Task.WhenAll(tasks);

            Logger.LogDebug("Cached play count for [{track}]", track.DisplayString());
        }
    }
}