using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class PlayCountPuller
    {
        private readonly IDatabaseAsync Cache;
        private readonly ILogger<PlayCountPuller> Logger;

        protected readonly ITrackApi TrackClient;
        protected readonly IAlbumApi AlbumClient;
        protected readonly IArtistApi ArtistClient;
        protected readonly IUserApi UserClient;

        public PlayCountPuller(
            ILogger<PlayCountPuller> logger,

            ITrackApi trackClient,
            IAlbumApi albumClient,
            IArtistApi artistClient,
            IUserApi userClient,
            IDatabaseAsync cache = null
        )
        {
            Cache = cache;
            Logger = logger;

            TrackClient = trackClient;
            AlbumClient = albumClient;
            ArtistClient = artistClient;
            UserClient = userClient;
        }

        public async Task<PlayCount> Get(string username, string track, string artist, string album, string albumArtist)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("No username provided");

            var trackCache = Cache?.StringGetAsync(Key.TrackPlayCount(track, artist));
            var albumCache = Cache?.StringGetAsync(Key.AlbumPlayCount(album, albumArtist));
            var artistCache = Cache?.StringGetAsync(Key.ArtistPlayCount(artist));
            var userCache = Cache?.StringGetAsync(Key.UserPlayCount(username));

            var cacheTasks = new Task[] { trackCache, albumCache, artistCache, userCache };

            await Task.WhenAll(cacheTasks.Where(t => t is not null));

            PlayCount playCount = new()
            {
                Username = username
            };

            Task<LastResponse<LastTrack>> trackHttp = null;
            Task<LastResponse<LastAlbum>> albumHttp = null;
            Task<LastResponse<LastArtist>> artistHttp = null;
            Task<LastResponse<LastUser>> userHttp = null;

            if (trackCache is not null && trackCache.IsCompletedSuccessfully && trackCache.Result != RedisValue.Null)
            {
                playCount.Track = (int) trackCache.Result;
            }
            else
            {
                trackHttp = TrackClient.GetInfoAsync(track, artist, username);
            }

            if (albumCache is not null && albumCache.IsCompletedSuccessfully && albumCache.Result != RedisValue.Null)
            {
                playCount.Album = (int) albumCache.Result;
            }
            else
            {
                albumHttp = AlbumClient.GetInfoAsync(albumArtist, album, username: username);
            }

            if (artistCache is not null && artistCache.IsCompletedSuccessfully && artistCache.Result != RedisValue.Null)
            {
                playCount.Artist = (int) artistCache.Result;
            }
            else
            {
                artistHttp = ArtistClient.GetInfoAsync(artist);
            }

            if (userCache is not null && userCache.IsCompletedSuccessfully && userCache.Result != RedisValue.Null)
            {
                playCount.User = (int) userCache.Result;
            }
            else
            {
                userHttp = UserClient.GetInfoAsync(username);
            }

            await Task.WhenAll(new Task[] {trackHttp, albumHttp, artistHttp, userHttp}.Where(t => t is not null));

            if (trackHttp is not null && trackHttp.IsCompletedSuccessfully)
            {
                if (trackHttp.Result.Success)
                {
                    playCount.Track = trackHttp.Result.Content.UserPlayCount;
                }
                else
                {
                    Logger.LogDebug($"Track info error [{username}] [{trackHttp.Result.Status}]");
                }
            }

            if (albumHttp is not null && albumHttp.IsCompletedSuccessfully)
            {
                if (albumHttp.Result.Success)
                {
                    playCount.Album = albumHttp.Result.Content.UserPlayCount;
                }
                else
                {
                    Logger.LogDebug($"Album info error [{username}] [{albumHttp.Result.Status}]");
                }
            }

            //TODO: Add artist count

            if (userHttp is not null && userHttp.IsCompletedSuccessfully)
            {
                if (userHttp.Result.Success)
                {
                    playCount.User = userHttp.Result.Content.Playcount;
                }
                else
                {
                    Logger.LogDebug($"User info error [{username}] [{userHttp.Result.Status}]");
                }
            }

            return playCount;
        }
    }
}
