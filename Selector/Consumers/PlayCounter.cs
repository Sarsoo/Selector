using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using SpotifyAPI.Web;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Api.Helpers;

namespace Selector
{
    public class PlayCounter : IPlayerConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly ITrackApi TrackClient;
        protected readonly IAlbumApi AlbumClient;
        protected readonly IArtistApi ArtistClient;
        protected readonly IUserApi UserClient;
        public readonly LastFmCredentials Credentials;
        protected readonly ILogger<PlayCounter> Logger;

        protected event EventHandler<PlayCount> NewPlayCount;

        public CancellationToken CancelToken { get; set; }

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        public PlayCounter(
            IPlayerWatcher watcher,
            ITrackApi trackClient,
            IAlbumApi albumClient,
            IArtistApi artistClient,
            IUserApi userClient,
            LastFmCredentials credentials = null,
            ILogger<PlayCounter> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            TrackClient = trackClient;
            AlbumClient = albumClient;
            ArtistClient = artistClient;
            UserClient = userClient;
            Credentials = credentials;
            Logger = logger ?? NullLogger<PlayCounter>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;
            
            Task.Run(async () => {
                try
                {
                    await AsyncCallback(e);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            using var scope = Logger.BeginScope(new Dictionary<string, object>() { { "spotify_username", e.SpotifyUsername }, { "id", e.Id }, { "username", Credentials.Username } });

            if (Credentials is null || string.IsNullOrWhiteSpace(Credentials.Username))
            {
                Logger.LogDebug("No Last.fm username, skipping play count");
                return;
            }

            if (e.Current.Item is FullTrack track)
            {
                using var trackScope = Logger.BeginScope(new Dictionary<string, object>() { { "track", track.DisplayString() } });

                Logger.LogTrace("Making Last.fm call");

                var trackInfo = TrackClient.GetInfoAsync(track.Name, track.Artists[0].Name, username: Credentials?.Username);
                var albumInfo = AlbumClient.GetInfoAsync(track.Album.Artists[0].Name, track.Album.Name, username: Credentials?.Username);
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
                    Logger.LogError(trackInfo.Exception, "Track info task faulted, [{context}]", e.Current.DisplayString());
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
                    Logger.LogError(albumInfo.Exception, "Album info task faulted, [{context}]", e.Current.DisplayString());
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
                    Logger.LogError(userInfo.Exception, "User info task faulted, [{context}]", e.Current.DisplayString());
                }

                Logger.LogDebug("Adding Last.fm data [{username}], track: {track_count}, album: {album_count}, artist: {artist_count}, user: {user_count}", Credentials.Username, trackCount, albumCount, artistCount, userCount);

                PlayCount playCount = new()
                {
                    Track = trackCount,
                    Album = albumCount,
                    Artist = artistCount,
                    User = userCount,
                    ListeningEvent = e
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

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange += Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
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
        public ListeningChangeEventArgs ListeningEvent { get; set; }
    }

    public class LastFmCredentials
    {
        public string Username { get; set; }
    }
}
