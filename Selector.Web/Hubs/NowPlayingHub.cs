using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Selector.Cache;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.Web.NowPlaying;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Web.Hubs
{
    public interface INowPlayingHubClient
    {
        public Task OnNewPlaying(CurrentlyPlayingDTO context);
        public Task OnNewAudioFeature(TrackAudioFeatures features);
        public Task OnNewPlayCount(PlayCount playCount);
        public Task OnNewCard(Card card);
    }

    public class NowPlayingHub: Hub<INowPlayingHubClient>
    {
        private readonly IDatabaseAsync Cache;
        private readonly AudioFeaturePuller AudioFeaturePuller;
        private readonly PlayCountPuller PlayCountPuller;
        private readonly ApplicationDbContext Db;
        private readonly IScrobbleRepository ScrobbleRepository;

        private readonly IOptions<NowPlayingOptions> nowOptions;

        public NowPlayingHub(
            IDatabaseAsync cache, 
            AudioFeaturePuller featurePuller, 
            ApplicationDbContext db,
            IScrobbleRepository scrobbleRepository,
            IOptions<NowPlayingOptions> options,
            PlayCountPuller playCountPuller = null
        )
        {
            Cache = cache;
            AudioFeaturePuller = featurePuller;
            PlayCountPuller = playCountPuller;
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
            var nowPlaying = await Cache.StringGetAsync(Key.CurrentlyPlaying(Context.UserIdentifier));
            if (nowPlaying != RedisValue.Null)
            {
                var deserialised = JsonSerializer.Deserialize(nowPlaying, JsonContext.Default.CurrentlyPlayingDTO);
                await Clients.Caller.OnNewPlaying(deserialised);
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

                if (user.LastFmConnected())
                {
                    var playCount = await PlayCountPuller.Get(user.LastFmUsername, track, artist, album, albumArtist);

                    if (user.ScrobbleSavingEnabled())
                    {
                        var artistScrobbles = ScrobbleRepository.GetAll(userId: user.Id, artistName: artist).ToArray();

                        playCount.Artist = artistScrobbles.Length;

                        playCount.ArtistCountData = artistScrobbles
                            //.Resample(nowOptions.Value.ArtistResampleWindow)
                            .ResampleByMonth()
                            .ToArray();

                        var postCalc = playCount.ArtistCountData.Select(s => s.Value).Sum();
                        Debug.Assert(postCalc == artistScrobbles.Count());

                        playCount.AlbumCountData = artistScrobbles
                            .Where(s => s.AlbumName.Equals(album, StringComparison.CurrentCultureIgnoreCase))
                            //.Resample(nowOptions.Value.AlbumResampleWindow)
                            .ResampleByMonth()
                            .ToArray();

                        playCount.TrackCountData = artistScrobbles
                            .Where(s => s.TrackName.Equals(track, StringComparison.CurrentCultureIgnoreCase))
                            //.Resample(nowOptions.Value.TrackResampleWindow)
                            .ResampleByMonth()
                            .ToArray();
                    }



                    await Clients.Caller.OnNewPlayCount(playCount);
                }
            }
        }

        public async Task SendFacts(string track, string artist, string album, string albumArtist)
        {
            var user = Db.Users
                        .AsNoTracking()
                        .Where(u => u.Id == Context.UserIdentifier)
                        .SingleOrDefault()
                            ?? throw new SqlNullValueException("No user returned");

            await PlayDensityFacts(user, track, artist, album, albumArtist);
        }

        public async Task PlayDensityFacts(ApplicationUser user, string track, string artist, string album, string albumArtist)
        {
            if (user.ScrobbleSavingEnabled())
            {
                var artistScrobbles = ScrobbleRepository.GetAll(userId: user.Id, artistName: artist, from: GetMaximumWindow()).ToArray();
                var artistDensity = artistScrobbles.Density(nowOptions.Value.ArtistDensityWindow);

                var tasks = new List<Task>(3);

                if (artistDensity > nowOptions.Value.ArtistDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new()
                    {
                        Content = $"You're on a {artist} binge! {artistDensity} plays/day recently"
                    }));
                }

                var albumDensity = artistScrobbles.Where(s => s.AlbumName.Equals(album, StringComparison.InvariantCultureIgnoreCase)).Density(nowOptions.Value.AlbumDensityWindow);

                if (albumDensity > nowOptions.Value.AlbumDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new()
                    {
                        Content = $"You're on a {album} binge! {albumDensity} plays/day recently"
                    }));
                }

                var trackDensity = artistScrobbles.Where(s => s.TrackName.Equals(track, StringComparison.InvariantCultureIgnoreCase)).Density(nowOptions.Value.TrackDensityWindow);

                if (albumDensity > nowOptions.Value.TrackDensityThreshold)
                {
                    tasks.Add(Clients.Caller.OnNewCard(new()
                    {
                        Content = $"You're on a {track} binge! {trackDensity} plays/day recently"
                    }));
                }

                if(tasks.Any())
                {
                    await Task.WhenAll(tasks);
                }
            }
        }

        private DateTime GetMaximumWindow() => GetMaximumWindow(new TimeSpan[] { nowOptions.Value.ArtistDensityWindow, nowOptions.Value.AlbumDensityWindow, nowOptions.Value.TrackDensityWindow });
        private DateTime GetMaximumWindow(IEnumerable<TimeSpan> windows) => windows.Select(w => DateTime.UtcNow - w).Min();
    }
}