using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Equality;
using SpotifyAPI.Web;

namespace Selector
{
    public class PlaylistWatcherConfig
    {
        public string PlaylistId { get; set; }
        public bool PullTracks { get; set; } = true;
    }

    public class PlaylistWatcher : BaseSpotifyWatcher, IPlaylistWatcher
    {
        new private readonly ILogger<PlaylistWatcher> Logger;
        private readonly ISpotifyClient spotifyClient;

        public PlaylistWatcherConfig config { get; set; }

        public event EventHandler<PlaylistChangeEventArgs> NetworkPoll;
        public event EventHandler<PlaylistChangeEventArgs> SnapshotChange;

        public event EventHandler<PlaylistChangeEventArgs> TracksAdded;
        public event EventHandler<PlaylistChangeEventArgs> TracksRemoved;

        public event EventHandler<PlaylistChangeEventArgs> NameChanged;
        public event EventHandler<PlaylistChangeEventArgs> DescriptionChanged;

        public FullPlaylist Live { get; private set; }
        private List<PlaylistTrack<IPlayableItem>> CurrentTracks { get; set; }
        private ICollection<PlaylistTrack<IPlayableItem>> LastAddedTracks { get; set; }
        private ICollection<PlaylistTrack<IPlayableItem>> LastRemovedTracks { get; set; }

        private FullPlaylist Previous { get; set; }
        public Timeline<FullPlaylist> Past { get; set; } = new();

        private IEqualityComparer<PlaylistTrack<IPlayableItem>> EqualityComparer = new PlayableItemEqualityComparer();

        public PlaylistWatcher(PlaylistWatcherConfig config,
            ISpotifyClient spotifyClient,
            ILogger<PlaylistWatcher> logger = null,
            int pollPeriod = 3000
        ) : base(logger)
        {
            this.spotifyClient = spotifyClient;
            this.config = config;
            Logger = logger ?? NullLogger<PlaylistWatcher>.Instance;
            PollPeriod = pollPeriod;
        }

        public override Task Reset()
        {
            Previous = null;
            Live = null;
            Past = new();

            return Task.CompletedTask;
        }

        public override async Task WatchOne(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            using var logScope = Logger.BeginScope(new Dictionary<string, object>
                { { "playlist_id", config.PlaylistId }, { "pull_tracks", config.PullTracks } });

            try
            {
                string id;

                if (config.PlaylistId.Contains(':'))
                {
                    id = config.PlaylistId.Split(':').Last();
                }
                else
                {
                    id = config.PlaylistId;
                }

                Logger.LogTrace("Making Spotify call");
                var polledCurrent = await spotifyClient.Playlists.Get(id);
                Logger.LogTrace("Received Spotify call [{context}]", polledCurrent?.DisplayString());

                if (polledCurrent != null) StoreCurrentPlaying(polledCurrent);

                // swap new item into live and bump existing down to previous
                Previous = Live;
                Live = polledCurrent;

                OnNetworkPoll(GetEvent());

                await CheckSnapshot();
                CheckStringValues();
            }
            catch (APIUnauthorizedException e)
            {
                Logger.LogDebug("Unauthorised error: [{message}] (should be refreshed and retried?)", e.Message);
                //throw e;
            }
            catch (APITooManyRequestsException e)
            {
                Logger.LogDebug("Too many requests error: [{message}]", e.Message);
                await Task.Delay(e.RetryAfter, token);
                // throw e;
            }
            catch (APIException e)
            {
                Logger.LogDebug("API error: [{message}]", e.Message);
                // throw e;
            }
        }

        private async Task CheckSnapshot()
        {
            switch (Previous, Live)
            {
                case (null, not null): // gone null
                    await PageLiveTracks();
                    break;
                case (not null, null): // went non-null
                    break;
                case (not null, not null): // continuing non-null

                    if (Live.SnapshotId != Previous.SnapshotId)
                    {
                        Logger.LogDebug("Snapshot Id changed: {previous} -> {current}", Previous.SnapshotId,
                            Live.SnapshotId);
                        await PageLiveTracks();
                        OnSnapshotChange(GetEvent());
                    }

                    break;
            }
        }

        private async Task PageLiveTracks()
        {
            if (config.PullTracks && Live.Tracks.Items.Count > 0)
            {
                Logger.LogDebug("Paging current tracks");

                var newCurrentTracks = await spotifyClient.Paginate(Live.Tracks).ToListAsync();

                Logger.LogTrace("Completed paging current tracks");

                if (CurrentTracks is not null)
                {
                    Logger.LogDebug("Identifying diffs");

                    LastAddedTracks = newCurrentTracks.Except(CurrentTracks, EqualityComparer).ToArray();
                    LastRemovedTracks = CurrentTracks.Except(newCurrentTracks, EqualityComparer).ToArray();

                    if (LastAddedTracks.Count > 0)
                    {
                        OnTracksAdded(GetEvent());
                    }

                    if (LastRemovedTracks.Count > 0)
                    {
                        OnTracksRemoved(GetEvent());
                    }

                    Logger.LogTrace("Completed identifying diffs");
                }

                CurrentTracks = newCurrentTracks;
            }
        }

        private PlaylistChangeEventArgs GetEvent() => PlaylistChangeEventArgs.From(Previous, Live, Past,
            tracks: CurrentTracks, addedTracks: LastAddedTracks, removedTracks: LastRemovedTracks, id: Id,
            username: SpotifyUsername);

        private void CheckStringValues()
        {
            switch (Previous, Live)
            {
                case (null, not null): // gone null
                    break;
                case (not null, null): // went non-null
                    break;
                case (not null, not null): // continuing non-null

                    if ((Previous, Live) is ({ Name: not null }, { Name: not null }))
                    {
                        if (!Live.Name.Equals(Previous.Name))
                        {
                            Logger.LogDebug("Name changed: {previous} -> {current}", Previous.SnapshotId,
                                Live.SnapshotId);
                            OnNameChanged(GetEvent());
                        }
                    }

                    if ((Previous, Live) is ({ Description: not null }, { Description: not null }))
                    {
                        if (!Live.Description.Equals(Previous.Description))
                        {
                            Logger.LogDebug("Description changed: {previous} -> {current}", Previous.SnapshotId,
                                Live.SnapshotId);
                            OnDescriptionChanged(GetEvent());
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Store currently playing in last plays. Determine whether new list or appending required
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreCurrentPlaying(FullPlaylist current)
        {
            Past?.Add(current);
        }

        #region Event Firers

        protected virtual void OnNetworkPoll(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing network poll event");

            NetworkPoll?.Invoke(this, args);
        }

        protected virtual void OnSnapshotChange(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing snapshot change event");

            SnapshotChange?.Invoke(this, args);
        }

        protected virtual void OnTracksAdded(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing tracks added event");

            TracksAdded?.Invoke(this, args);
        }

        protected virtual void OnTracksRemoved(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing tracks removed event");

            TracksRemoved?.Invoke(this, args);
        }

        protected virtual void OnNameChanged(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing name changed event");

            NameChanged?.Invoke(this, args);
        }

        protected virtual void OnDescriptionChanged(PlaylistChangeEventArgs args)
        {
            Logger.LogTrace("Firing description changed event");

            DescriptionChanged?.Invoke(this, args);
        }

        #endregion
    }
}