using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using SpotifyAPI.Web;
using IF.Lastfm.Core.Api;

namespace Selector
{
    public class PlayCounter : IConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly ITrackApi TrackClient;
        protected readonly IAlbumApi AlbumClient;
        protected readonly IArtistApi ArtistClient;
        protected readonly IUserApi UserClient;
        protected readonly LastFmCredentials Credentials;
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
            
            Task.Run(async () => { await AsyncCallback(e); }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            if (e.Current.Item is FullTrack track)
            {
                Logger.LogTrace("Making Last.fm call");

                var trackInfo = TrackClient.GetInfoAsync(track.Name, track.Artists[0].Name, username: Credentials?.Username);
                var albumInfo = AlbumClient.GetInfoAsync(track.Album.Artists[0].Name, track.Album.Name, username: Credentials?.Username);
                var artistInfo = ArtistClient.GetInfoAsync(track.Artists[0].Name);
                // TODO: Null checking on credentials
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
                        Logger.LogDebug($"Track info error [{e.Id}/{e.SpotifyUsername}] [{trackInfo.Result.Status}]");
                    }
                }
                else
                {
                    Logger.LogError(trackInfo.Exception, $"Track info task faulted, [{e.Id}/{e.SpotifyUsername}] [{e.Current.DisplayString()}]");
                }

                if (albumInfo.IsCompletedSuccessfully)
                {
                    if (albumInfo.Result.Success)
                    {
                        albumCount = albumInfo.Result.Content.UserPlayCount;
                    }
                    else
                    {
                        Logger.LogDebug($"Album info error [{e.Id}/{e.SpotifyUsername}] [{albumInfo.Result.Status}]");
                    }
                }
                else
                {
                    Logger.LogError(albumInfo.Exception, $"Album info task faulted, [{e.Id}/{e.SpotifyUsername}] [{e.Current.DisplayString()}]");
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
                        Logger.LogDebug($"User info error [{e.Id}/{e.SpotifyUsername}] [{userInfo.Result.Status}]");
                    }
                }
                else
                {
                    Logger.LogError(userInfo.Exception, $"User info task faulted, [{e.Id}/{e.SpotifyUsername}] [{e.Current.DisplayString()}]");
                }

                Logger.LogDebug($"Adding Last.fm data [{e.Id}/{e.SpotifyUsername}/{Credentials.Username}] [{track.DisplayString()}], track: {trackCount}, album: {albumCount}, artist: {artistCount}, user: {userCount}");

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
                Logger.LogDebug($"Ignoring podcast episdoe [{episode.DisplayString()}]");
            }
            else
            {
                Logger.LogError($"Unknown item pulled from API [{e.Current.Item}]");
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
        public ListeningChangeEventArgs ListeningEvent { get; set; }
    }

    public class LastFmCredentials
    {
        public string Username { get; set; }
    }
}
