using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector
{
    public class PlayerWatcher: BaseWatcher, IPlayerWatcher
    {
        new private readonly ILogger<PlayerWatcher> Logger;
        private readonly IPlayerClient spotifyClient;
        private readonly IEqual eq;

        public event EventHandler<ListeningChangeEventArgs> NetworkPoll;
        public event EventHandler<ListeningChangeEventArgs> ItemChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;
        public event EventHandler<ListeningChangeEventArgs> ContentChange;

        public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        public event EventHandler<ListeningChangeEventArgs> DeviceChange;
        public event EventHandler<ListeningChangeEventArgs> PlayingChange;

        public CurrentlyPlayingContext Live { get; private set; }
        private CurrentlyPlayingContext Previous { get; set; }
        public PlayerTimeline Past { get; set; } = new();

        public PlayerWatcher(IPlayerClient spotifyClient, 
                IEqual equalityChecker,
                ILogger<PlayerWatcher> logger = null,
                int pollPeriod = 3000
        ) : base(logger) {

            this.spotifyClient = spotifyClient;
            eq = equalityChecker;
            Logger = logger ?? NullLogger<PlayerWatcher>.Instance;
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
            
            try{
                Logger.LogTrace("Making Spotify call");
                var polledCurrent = await spotifyClient.GetCurrentPlayback();
                Logger.LogTrace($"Received Spotify call [{polledCurrent?.DisplayString()}]");

                if (polledCurrent != null) StoreCurrentPlaying(polledCurrent);

                // swap new item into live and bump existing down to previous
                Previous = Live;
                Live = polledCurrent;

                OnNetworkPoll(GetEvent());

                // NOT PLAYING
                if(Previous is null && Live is null)
                {
                    // Console.WriteLine("not playing");
                }
                else
                {
                    // STARTED PLAYBACK
                    if(Previous is null && Live is not null)
                    {
                        Logger.LogDebug($"Playback started: {Live.DisplayString()}");
                        OnPlayingChange(GetEvent());
                        OnItemChange(GetEvent());
                        OnContextChange(GetEvent());
                    }
                    // STOPPED PLAYBACK
                    else if(Previous is not null && Live is null)
                    {
                        Logger.LogDebug($"Playback stopped: {Previous.DisplayString()}");
                        OnPlayingChange(GetEvent());
                        OnItemChange(GetEvent());
                        OnContextChange(GetEvent());
                    }
                    // CONTINUING PLAYBACK
                    else {

                        // MUSIC
                        if(Previous.Item is FullTrack previousTrack 
                            && Live.Item is FullTrack currentTrack)
                        {
                            if(!eq.IsEqual(previousTrack, currentTrack)) {
                                Logger.LogDebug($"Track changed: {previousTrack.DisplayString()} -> {currentTrack.DisplayString()}");
                                OnItemChange(GetEvent());
                            }

                            if(!eq.IsEqual(previousTrack.Album, currentTrack.Album)) {
                                Logger.LogDebug($"Album changed: {previousTrack.Album.DisplayString()} -> {currentTrack.Album.DisplayString()}");
                                OnAlbumChange(GetEvent());
                            }

                            if(!eq.IsEqual(previousTrack.Artists[0], currentTrack.Artists[0])) {
                                Logger.LogDebug($"Artist changed: {previousTrack.Artists.DisplayString()} -> {currentTrack.Artists.DisplayString()}");
                                OnArtistChange(GetEvent());
                            }
                        }
                        // CHANGED CONTENT TYPE
                        else if((Previous.Item is FullTrack && Live.Item is FullEpisode)
                            || (Previous.Item is FullEpisode && Live.Item is FullTrack))
                        {
                            Logger.LogDebug($"Media type changed: {Previous.Item}, {Previous.Item}");
                            OnContentChange(GetEvent());
                            OnItemChange(GetEvent());
                        }
                        // PODCASTS
                        else if(Previous.Item is FullEpisode previousEp 
                            && Live.Item is FullEpisode currentEp)
                        {
                            if(!eq.IsEqual(previousEp, currentEp)) {
                                Logger.LogDebug($"Podcast changed: {previousEp.DisplayString()} -> {currentEp.DisplayString()}");
                                OnItemChange(GetEvent());
                            }
                        }
                        else {
                            Logger.LogError($"Unknown combination of previous and current playing contexts, [{Previous.DisplayString()}] [{Live.DisplayString()}]");
                            throw new NotSupportedException("Unknown item combination");
                        }

                        // CONTEXT
                        if(!eq.IsEqual(Previous.Context, Live.Context)) {
                            Logger.LogDebug($"Context changed: {Previous.Context.DisplayString()} -> {Live.Context.DisplayString()}");
                            OnContextChange(GetEvent());
                        }

                        // DEVICE
                        if(!eq.IsEqual(Previous?.Device, Live?.Device)) {
                            Logger.LogDebug($"Device changed: {Previous?.Device.DisplayString()} -> {Live?.Device.DisplayString()}");
                            OnDeviceChange(GetEvent());
                        }

                        // IS PLAYING
                        if(Previous.IsPlaying != Live.IsPlaying) {
                            Logger.LogDebug($"Playing state changed: {Previous.IsPlaying} -> {Live.IsPlaying}");
                            OnPlayingChange(GetEvent());
                        }

                        // VOLUME
                        if(Previous.Device.VolumePercent != Live.Device.VolumePercent) {
                            Logger.LogDebug($"Volume changed: {Previous.Device.VolumePercent}% -> {Live.Device.VolumePercent}%");
                            OnVolumeChange(GetEvent());
                        }
                    }
                }
            }
            catch(APIUnauthorizedException e)
            {
                Logger.LogDebug($"Unauthorised error: [{e.Message}] (should be refreshed and retried?)");
                //throw e;
            }
            catch(APITooManyRequestsException e)
            {
                Logger.LogDebug($"Too many requests error: [{e.Message}]");
                await Task.Delay(e.RetryAfter);
                // throw e;
            }
            catch(APIException e)
            {
                Logger.LogDebug($"API error: [{e.Message}]");
                // throw e;
            }
        }

        private ListeningChangeEventArgs GetEvent() => ListeningChangeEventArgs.From(Previous, Live, Past, id: Id, username: SpotifyUsername);

        /// <summary>
        /// Store currently playing in last plays. Determine whether new list or appending required
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreCurrentPlaying(CurrentlyPlayingContext current) 
        {
            Past?.Add(current);
        }

        #region Event Firers
        protected virtual void OnNetworkPoll(ListeningChangeEventArgs args)
        {
            NetworkPoll?.Invoke(this, args);
        }

        protected virtual void OnItemChange(ListeningChangeEventArgs args)
        {
            ItemChange?.Invoke(this, args); 
        }

        protected virtual void OnAlbumChange(ListeningChangeEventArgs args)
        {
            AlbumChange?.Invoke(this, args); 
        }

        protected virtual void OnArtistChange(ListeningChangeEventArgs args)
        {
            ArtistChange?.Invoke(this, args); 
        }

        protected virtual void OnContextChange(ListeningChangeEventArgs args)
        {
            ContextChange?.Invoke(this, args); 
        }

        protected virtual void OnContentChange(ListeningChangeEventArgs args)
        {
            ContentChange?.Invoke(this, args); 
        }

        protected virtual void OnVolumeChange(ListeningChangeEventArgs args)
        {
            VolumeChange?.Invoke(this, args);
        }

        protected virtual void OnDeviceChange(ListeningChangeEventArgs args)
        {
            DeviceChange?.Invoke(this, args); 
        }

        protected virtual void OnPlayingChange(ListeningChangeEventArgs args)
        {
            PlayingChange?.Invoke(this, args); 
        }

        #endregion
    }
}
