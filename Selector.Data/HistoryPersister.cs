using System.Text.Json;
using Microsoft.Extensions.Logging;
using Selector.Cache;
using Selector.Model;
using static SpotifyAPI.Web.PlaylistRemoveItemsRequest;

namespace Selector.Data;

public class HistoryPersisterConfig
{
    public string Username { get; set; }
    public bool InitialClear { get; set; } = true;
    public bool Apply50PercentRule { get; set; } = false;
}

public class HistoryPersister
{
    private HistoryPersisterConfig Config { get; set; }
    private ApplicationDbContext Db { get; set; }
    private DataJsonContext Json { get; set; }

    private DurationPuller DurationPuller { get; set; }

    private ILogger<HistoryPersister> Logger { get; set; }

    private readonly Dictionary<string, int> Durations;

    public HistoryPersister(
        ApplicationDbContext db,
        DataJsonContext json,
        HistoryPersisterConfig config,
        DurationPuller durationPuller = null,
        ILogger<HistoryPersister> logger = null)
    {
        Config = config;
        Db = db;
        Json = json;
        DurationPuller = durationPuller;
        Logger = logger;

        if (config.Apply50PercentRule && DurationPuller is null)
        {
            throw new ArgumentNullException(nameof(DurationPuller));
        }

        Durations = new();
    }

    public void Process(string input)
    {
        var parsed = JsonSerializer.Deserialize(input, Json.EndSongArray);
        Process(parsed).Wait();
    }

    public async Task Process(Stream input)
    {
        var parsed = await JsonSerializer.DeserializeAsync(input, Json.EndSongArray);
        await Process(parsed);
    }

    public async Task Process(IEnumerable<Stream> input)
    {
        var songs = Enumerable.Empty<EndSong>();

        foreach(var singleInput in input)
        {
            var parsed = await JsonSerializer.DeserializeAsync(singleInput, Json.EndSongArray);
            songs = songs.Concat(parsed);

            Logger?.LogDebug("Parsed {:n0} items for {}", parsed.Length, Config.Username);
        }

        await Process(songs);
    }

    public async Task Process(IEnumerable<EndSong> input)
    {
        var user = Db.Users.Single(u => u.UserName == Config.Username);

        if (Config.InitialClear)
        {
            var latestTime = input.OrderBy(x => x.ts).Last().ts;
            var time = DateTime.Parse(latestTime).ToUniversalTime();
            Db.SpotifyListen.RemoveRange(Db.SpotifyListen.Where(x => x.UserId == user.Id && x.Timestamp <= time));
        }

        var filtered = input.Where(x => x.ms_played > 30000
                                     && !string.IsNullOrWhiteSpace(x.master_metadata_track_name))
                            .DistinctBy(x => (x.offline_timestamp, x.ts, x.spotify_track_uri))
                            .ToArray();

        Logger.LogInformation("{:n0} items after filtering", filtered.Length);

        var processedCounter = 0;
        foreach (var item in filtered.Chunk(1000))
        {
            IEnumerable<EndSong> toPopulate = item;

            if (Config.Apply50PercentRule)
            {
                Logger.LogDebug("Validating tracks {:n0}/{:n0}", processedCounter + 1, filtered.Length);

                toPopulate = Passes50PcRule(toPopulate);
            }

            Db.SpotifyListen.AddRange(toPopulate.Select(x => new SpotifyListen()
            {
                TrackName = x.master_metadata_track_name,
                AlbumName = x.master_metadata_album_album_name,
                ArtistName = x.master_metadata_album_artist_name,

                Timestamp = DateTime.Parse(x.ts).ToUniversalTime(),
                PlayedDuration = x.ms_played,

                TrackUri = x.spotify_track_uri,
                UserId = user.Id
            }));

            processedCounter += item.Length;
        }

        Logger?.LogInformation("Added {:n0} historical items for {}", processedCounter, user.UserName);

        await Db.SaveChangesAsync();
    }

    private const int FOUR_MINUTES = 4 * 60 * 1000;

    public async Task<bool> Passes50PcRule(EndSong song)
    {
        if (string.IsNullOrWhiteSpace(song.spotify_track_uri)) return true;

        int duration;

        if (Durations.TryGetValue(song.spotify_track_uri, out duration))
        {

        }
        else
        {
            var pulledDuration = await DurationPuller.Get(song.spotify_track_uri);

            if (pulledDuration is int d)
            {
                duration = d;

                Durations.Add(song.spotify_track_uri, duration);
            }
            else
            {
                Logger.LogDebug("No duration returned for {}/{}", song.master_metadata_track_name, song.master_metadata_album_artist_name);
                return true; // if can't get duration, just pass
            }
        }

        return CheckDuration(song, duration);
    }

    public IEnumerable<EndSong> Passes50PcRule(IEnumerable<EndSong> inputTracks)
    {
        var toPullOverWire = new List<EndSong>();

        // quick return items from local cache
        foreach(var track in inputTracks)
        {
            if (string.IsNullOrWhiteSpace(track.spotify_track_uri)) yield return track;

            if (Durations.TryGetValue(track.spotify_track_uri, out var duration))
            {
                if (CheckDuration(track, duration))
                {
                    yield return track;
                }
            }
            else
            {
                toPullOverWire.Add(track);
            }
        }

        var pulledDuration = DurationPuller.Get(toPullOverWire.Select(x => x.spotify_track_uri)).Result;

        // apply results to cache
        foreach((var uri, var dur) in pulledDuration)
        {
            Durations[uri] = dur;
        }

        // check return acceptable tracks from pulled
        foreach(var track in toPullOverWire)
        {
            if(pulledDuration.TryGetValue(track.spotify_track_uri, out var duration))
            {
                if(CheckDuration(track, duration))
                {
                    yield return track;
                }
            }
            else
            {
                yield return track;
            }
        }
    }

    public bool CheckDuration(EndSong song, int duration) => song.ms_played >= duration / 2 || song.ms_played >= FOUR_MINUTES;
}

