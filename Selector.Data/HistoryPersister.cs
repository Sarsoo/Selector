using System;
using System.Text.Json;
using Selector.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

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

    private ILogger<HistoryPersister> Logger { get; set; }

    public HistoryPersister(ApplicationDbContext db, DataJsonContext json, HistoryPersisterConfig config, ILogger<HistoryPersister> logger = null)
    {
        Config = config;
        Db = db;
        Json = json;
        Logger = logger;
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

            Logger?.LogDebug("Parsed {} items for {}", parsed.Length, Config.Username);
        }

        await Process(songs);
    }

    public async Task Process(IEnumerable<EndSong> input)
    {
        if (Config.InitialClear)
        {
            Db.SpotifyListen.RemoveRange(Db.SpotifyListen.Where(x => x.User.UserName == Config.Username));
        }

        var user = Db.Users.Single(u => u.UserName == Config.Username);

        var counter = 0;

        foreach(var item in input)
        {
            if(!string.IsNullOrWhiteSpace(item.master_metadata_track_name))
            {
                Db.SpotifyListen.Add(new()
                {
                    TrackName = item.master_metadata_track_name,
                    AlbumName = item.master_metadata_album_album_name,
                    ArtistName = item.master_metadata_album_artist_name,

                    Timestamp = DateTime.Parse(item.ts).ToUniversalTime(),
                    PlayedDuration = item.ms_played,

                    TrackUri = item.spotify_track_uri,
                    UserId = user.Id
                });

                counter++;
            }
        }

        Logger?.LogInformation("Added {} historical items for {}", counter, user.UserName);

        await Db.SaveChangesAsync();
    }
}

