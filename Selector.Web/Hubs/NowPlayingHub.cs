using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using SpotifyAPI.Web;
using StackExchange.Redis;

using Selector.Cache;
using Selector.Model;

namespace Selector.Web.Hubs
{
    public interface INowPlayingHubClient
    {
        public Task OnNewPlaying(CurrentlyPlayingDTO context);
        public Task OnNewAudioFeature(TrackAudioFeatures features);
        public Task OnNewPlayCount(PlayCount playCount);
    }

    public class NowPlayingHub: Hub<INowPlayingHubClient>
    {
        private readonly IDatabaseAsync Cache;
        private readonly AudioFeaturePuller AudioFeaturePuller;
        private readonly PlayCountPuller PlayCountPuller;
        private readonly ApplicationDbContext Db;

        public NowPlayingHub(
            IDatabaseAsync cache, 
            AudioFeaturePuller featurePuller, 
            ApplicationDbContext db,
            PlayCountPuller playCountPuller = null
        )
        {
            Cache = cache;
            AudioFeaturePuller = featurePuller;
            PlayCountPuller = playCountPuller;
            Db = db;
        }

        public async Task OnConnected()
        {
            await SendNewPlaying();
        }

        public async Task SendNewPlaying()
        {
            var nowPlaying = await Cache.StringGetAsync(Key.CurrentlyPlaying(Context.UserIdentifier));
            if (nowPlaying != RedisValue.Null)
            {
                var deserialised = JsonSerializer.Deserialize(nowPlaying, JsonContext.Default.CurrentlyPlayingDTO);
                await Clients.Caller.OnNewPlaying(deserialised);
            }
        }

        public async Task SendAudioFeatures(string trackId)
        {
            var user = Db.Users
                        .AsNoTracking()
                        .Where(u => u.Id == Context.UserIdentifier)
                        .SingleOrDefault() 
                            ?? throw new SqlNullValueException("No user returned");
            var watcher = Db.Watcher
                        .AsNoTracking()
                        .Where(w => w.UserId == Context.UserIdentifier
                                && w.Type == WatcherType.Player)
                        .SingleOrDefault() 
                            ?? throw new SqlNullValueException($"No player watcher found for [{user.UserName}]");
            
            var feature = await AudioFeaturePuller.Get(user.SpotifyRefreshToken, trackId);

            if (feature is not null)
            {
                await Clients.Caller.OnNewAudioFeature(feature);
            }
        }

        public async Task SendPlayCount(string track, string artist, string album, string albumArtist)
        {
            if(PlayCountPuller is not null)
            {
                var user = Db.Users
                        .AsNoTracking()
                        .Where(u => u.Id == Context.UserIdentifier)
                        .SingleOrDefault()
                            ?? throw new SqlNullValueException("No user returned");

                if(!string.IsNullOrWhiteSpace(user.LastFmUsername))
                {
                    var playCount = await PlayCountPuller.Get(user.LastFmUsername, track, artist, album, albumArtist);

                    if (playCount is not null)
                    {
                        await Clients.Caller.OnNewPlayCount(playCount);
                    }
                }
            }
        }
    }
}