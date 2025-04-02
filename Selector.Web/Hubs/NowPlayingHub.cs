using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Selector.AppleMusic;
using Selector.Cache;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.SignalR;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using StackExchange.Redis;

namespace Selector.Web.Hubs
{
    public class NowPlayingHub : Hub<INowPlayingHubClient>, INowPlayingHub
    {
        private readonly IDatabaseAsync Cache;
        private readonly AudioFeaturePuller AudioFeaturePuller;
        private readonly PlayCountPuller PlayCountPuller;
        private readonly DBPlayCountPuller DBPlayCountPuller;
        private readonly ApplicationDbContext Db;
        private readonly IScrobbleRepository ScrobbleRepository;

        private readonly IOptions<NowPlayingOptions> nowOptions;

        public NowPlayingHub(
            IDatabaseAsync cache,
            AudioFeaturePuller featurePuller,
            ApplicationDbContext db,
            IScrobbleRepository scrobbleRepository,
            IOptions<NowPlayingOptions> options,
            DBPlayCountPuller dbPlayCountPuller,
            PlayCountPuller playCountPuller = null
        )
        {
            Cache = cache;
            AudioFeaturePuller = featurePuller;
            PlayCountPuller = playCountPuller;
            DBPlayCountPuller = dbPlayCountPuller;
            Db = db;
            ScrobbleRepository = scrobbleRepository;
            nowOptions = options;
        }

        public async Task OnConnected()
        {
            await SendNewPlaying();
        }

        public async Task SendNewPlaying()
        {
            var nowPlaying = await Cache.StringGetAsync(Key.CurrentlyPlayingSpotify(Context.UserIdentifier));
            if (nowPlaying != RedisValue.Null)
            {
                var deserialised =
                    JsonSerializer.Deserialize(nowPlaying, SpotifyJsonContext.Default.SpotifyCurrentlyPlayingDTO);
                await Clients.Caller.OnNewPlayingSpotify(deserialised);
            }

            var nowPlayingApple = await Cache.StringGetAsync(Key.CurrentlyPlayingAppleMusic(Context.UserIdentifier));
            if (nowPlayingApple != RedisValue.Null)
            {
                var deserialised =
                    JsonSerializer.Deserialize(nowPlayingApple, AppleJsonContext.Default.AppleCurrentlyPlayingDTO);
                await Clients.Caller.OnNewPlayingApple(deserialised);
            }
        }

        public async Task SendAudioFeatures(string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId)) return;

            var user = Db.Users
                           .AsNoTracking()
                           .Where(u => u.Id == Context.UserIdentifier)
                           .SingleOrDefault()
                       ?? throw new SqlNullValueException("No user returned");
            var watcher = Db.Watcher
                              .AsNoTracking()
                              .Where(w => w.UserId == Context.UserIdentifier
                                          && w.Type == WatcherType.SpotifyPlayer)
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
            if (PlayCountPuller is not null)
            {
                var user = Db.Users
                               .AsNoTracking()
                               .Where(u => u.Id == Context.UserIdentifier)
                               .SingleOrDefault()
                           ?? throw new SqlNullValueException("No user returned");

                if (user.LastFmConnected())
                {
                    PlayCount playCount;

                    if (user.ScrobbleSavingEnabled())
                    {
                        playCount = await DBPlayCountPuller.Get(user.UserName, track, artist, album, albumArtist);
                    }
                    else
                    {
                        playCount = await PlayCountPuller.Get(user.LastFmUsername, track, artist, album, albumArtist);
                    }

                    await Clients.Caller.OnNewPlayCount(playCount);
                }
            }
        }

        public async Task SendFacts(string track, string artist, string album, string albumArtist)
        {
            await PlayDensityFacts(track, artist, album, albumArtist);
        }

        public async Task PlayDensityFacts(string track, string artist, string album, string albumArtist)
        {
            var user = await Db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Context.UserIdentifier);

            if (user.ScrobbleSavingEnabled())
            {
                var artistScrobbles = ScrobbleRepository
                    .GetAll(userId: user.Id, artistName: artist, from: GetMaximumWindow()).ToArray();
                var artistDensity = artistScrobbles.Density(nowOptions.Value.ArtistDensityWindow);

                var tasks = new List<Task>(3);

                if (artistDensity > nowOptions.Value.ArtistDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new Card()
                    {
                        Content = $"You're on a {artist} binge! {artistDensity} plays/day recently"
                    }));
                }

                var albumDensity = artistScrobbles
                    .Where(s => s.AlbumName.Equals(album, StringComparison.InvariantCultureIgnoreCase))
                    .Density(nowOptions.Value.AlbumDensityWindow);

                if (albumDensity > nowOptions.Value.AlbumDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new Card()
                    {
                        Content = $"You're on a {album} binge! {albumDensity} plays/day recently"
                    }));
                }

                var trackDensity = artistScrobbles
                    .Where(s => s.TrackName.Equals(track, StringComparison.InvariantCultureIgnoreCase))
                    .Density(nowOptions.Value.TrackDensityWindow);

                if (albumDensity > nowOptions.Value.TrackDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new Card()
                    {
                        Content = $"You're on a {track} binge! {trackDensity} plays/day recently"
                    }));
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }
            }
        }

        private DateTime GetMaximumWindow() => GetMaximumWindow(new TimeSpan[]
        {
            nowOptions.Value.ArtistDensityWindow, nowOptions.Value.AlbumDensityWindow,
            nowOptions.Value.TrackDensityWindow
        });

        private DateTime GetMaximumWindow(IEnumerable<TimeSpan> windows) =>
            windows.Select(w => DateTime.UtcNow - w).Min();
    }
}