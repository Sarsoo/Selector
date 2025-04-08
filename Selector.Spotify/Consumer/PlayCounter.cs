using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Extensions;
using Selector.Spotify.Timeline;
using SpotifyAPI.Web;

namespace Selector.Spotify.Consumer
{
    public class PlayCounter(
        ISpotifyPlayerWatcher watcher,
        ITrackApi trackClient,
        IAlbumApi albumClient,
        IArtistApi artistClient,
        IUserApi userClient,
        LastFmCredentials? credentials = null,
        ILogger<PlayCounter>? logger = null,
        CancellationToken token = default)
        : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>(watcher, logger),
            ISpotifyPlayerConsumer
    {
        protected readonly ISpotifyPlayerWatcher Watcher = watcher;
        protected readonly ITrackApi TrackClient = trackClient;
        protected readonly IAlbumApi AlbumClient = albumClient;
        protected readonly IArtistApi ArtistClient = artistClient;
        protected readonly IUserApi UserClient = userClient;
        public readonly LastFmCredentials? Credentials = credentials;
        protected new readonly ILogger<PlayCounter> Logger = logger ?? NullLogger<PlayCounter>.Instance;

        protected event EventHandler<PlayCount>? NewPlayCount;

        public CancellationToken CancelToken { get; set; } = token;

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            using var scope = Logger.BeginScope(new Dictionary<string, object>()
                { { "spotify_username", e.SpotifyUsername }, { "id", e.Id }, { "username", Credentials.Username } });

            if (Credentials is null || string.IsNullOrWhiteSpace(Credentials.Username))
            {
                Logger.LogDebug("No Last.fm username, skipping play count");
                return;
            }

            if (e.Current.Item is FullTrack track)
            {
                using var trackScope = Logger.BeginScope(new Dictionary<string, object>()
                    { { "track", track.DisplayString() } });

                Logger.LogTrace("Making Last.fm call");

                var trackInfo =
                    TrackClient.GetInfoAsync(track.Name, track.Artists[0].Name, username: Credentials?.Username);
                var albumInfo = AlbumClient.GetInfoAsync(track.Album.Artists[0].Name, track.Album.Name,
                    username: Credentials?.Username);
                var artistInfo = ArtistClient.GetInfoAsync(track.Artists[0].Name);
                var userInfo = UserClient.GetInfoAsync(Credentials.Username);

                await Task.WhenAll(new Task[] { trackInfo, albumInfo, artistInfo, userInfo });

                int? trackCount = null, albumCount = null, artistCount = null, userCount = null;

                if (trackInfo.IsCompletedSuccessfully)
                {
                    if (trackInfo.Result.Success)
                    {
                        trackCount = trackInfo.Result.Content.UserPlayCount;
                    }
                    else
                    {
                        Logger.LogDebug("Track info error [{status}]", trackInfo.Result.Status);
                    }
                }
                else
                {
                    Logger.LogError(trackInfo.Exception, "Track info task faulted, [{context}]",
                        e.Current.DisplayString());
                }

                if (albumInfo.IsCompletedSuccessfully)
                {
                    if (albumInfo.Result.Success)
                    {
                        albumCount = albumInfo.Result.Content.UserPlayCount;
                    }
                    else
                    {
                        Logger.LogDebug("Album info error [{status}]", albumInfo.Result.Status);
                    }
                }
                else
                {
                    Logger.LogError(albumInfo.Exception, "Album info task faulted, [{context}]",
                        e.Current.DisplayString());
                }

                //TODO: Add artist count

                if (userInfo.IsCompletedSuccessfully)
                {
                    if (userInfo.Result.Success)
                    {
                        userCount = userInfo.Result.Content.Playcount;
                    }
                    else
                    {
                        Logger.LogDebug("User info error [{status}]", userInfo.Result.Status);
                    }
                }
                else
                {
                    Logger.LogError(userInfo.Exception, "User info task faulted, [{context}]",
                        e.Current.DisplayString());
                }

                Logger.LogDebug(
                    "Adding Last.fm data [{username}], track: {track_count}, album: {album_count}, artist: {artist_count}, user: {user_count}",
                    Credentials.Username, trackCount, albumCount, artistCount, userCount);

                PlayCount playCount = new()
                {
                    Track = trackCount,
                    Album = albumCount,
                    Artist = artistCount,
                    User = userCount,
                    SpotifyListeningEvent = e
                };

                if (!string.IsNullOrWhiteSpace(Credentials.Username))
                    playCount.Username = Credentials.Username;

                OnNewPlayCount(playCount);
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                Logger.LogDebug("Ignoring podcast episdoe [{episode}]", episode.DisplayString());
            }
            else if (e.Current.Item is null)
            {
                Logger.LogDebug("Skipping play count pulling for null item [{context}]", e.Current.DisplayString());
            }
            else
            {
                Logger.LogError("Unknown item pulled from API [{item}]", e.Current.Item);
            }
        }

        protected virtual void OnNewPlayCount(PlayCount args)
        {
            NewPlayCount?.Invoke(this, args);
        }
    }

    public class PlayCount
    {
        public int? Track { get; set; }
        public int? Album { get; set; }
        public int? Artist { get; set; }
        public int? User { get; set; }
        public string Username { get; set; }
        public IEnumerable<CountSample> TrackCountData { get; set; }
        public IEnumerable<CountSample> AlbumCountData { get; set; }
        public IEnumerable<CountSample> ArtistCountData { get; set; }
        public SpotifyListeningChangeEventArgs SpotifyListeningEvent { get; set; }
    }

    public class LastFmCredentials
    {
        public string Username { get; set; }
    }
}