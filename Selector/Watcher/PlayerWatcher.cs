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

        public event EventHandler<ListeningChangeEventArgs> ItemChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;
        public event EventHandler<ListeningChangeEventArgs> ContentChange;

        public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        public event EventHandler<ListeningChangeEventArgs> DeviceChange;
        public event EventHandler<ListeningChangeEventArgs> PlayingChange;

        public CurrentlyPlayingContext Live { get; private set; }
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

        public override async Task WatchOne(CancellationToken token = default) 
        {
            token.ThrowIfCancellationRequested();
            
            try{
                Logger.LogTrace("Making Spotify call");
                var polledCurrent = await spotifyClient.GetCurrentPlayback();
                Logger.LogTrace($"Received Spotify call [{polledCurrent?.DisplayString()}]");

                if (polledCurrent != null) StoreCurrentPlaying(polledCurrent);

                // swap new item into live and bump existing down to previous
                CurrentlyPlayingContext previous;
                if(Live is null) {
                    Live = polledCurrent;
                    previous = polledCurrent;
                }
                else {
                    previous = Live;
                    Live = polledCurrent;
                }

                // NOT PLAYING
                if(previous is null && Live is null)
                {
                    // Console.WriteLine("not playing");
                }
                else
                {
                    // STARTED PLAYBACK
                    if(previous is null 
                        && (Live.Item is FullTrack || Live.Item is FullEpisode))
                    {
                        Logger.LogDebug($"Playback started: {Live.DisplayString()}");
                        OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                    }
                    // STOPPED PLAYBACK
                    else if((previous.Item is FullTrack || previous.Item is FullEpisode) 
                        && Live is null)
                    {
                        Logger.LogDebug($"Playback stopped: {previous.DisplayString()}");
                        OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                    }
                    // CONTINUING PLAYBACK
                    else {

                        // MUSIC
                        if(previous.Item is FullTrack previousTrack 
                            && Live.Item is FullTrack currentTrack)
                        {
                            if(!eq.IsEqual(previousTrack, currentTrack)) {
                                Logger.LogDebug($"Track changed: {previousTrack.DisplayString()} -> {currentTrack.DisplayString()}");
                                OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                            }

                            if(!eq.IsEqual(previousTrack.Album, currentTrack.Album)) {
                                Logger.LogDebug($"Album changed: {previousTrack.Album.DisplayString()} -> {currentTrack.Album.DisplayString()}");
                                OnAlbumChange(ListeningChangeEventArgs.From(previous, Live));
                            }

                            if(!eq.IsEqual(previousTrack.Artists[0], currentTrack.Artists[0])) {
                                Logger.LogDebug($"Artist changed: {previousTrack.Artists.DisplayString()} -> {currentTrack.Artists.DisplayString()}");
                                OnArtistChange(ListeningChangeEventArgs.From(previous, Live));
                            }
                        }
                        // CHANGED CONTENT
                        else if((previous.Item is FullTrack && Live.Item is FullEpisode)
                            || (previous.Item is FullEpisode && Live.Item is FullTrack))
                        {
                            Logger.LogDebug($"Media type changed: {previous.Item}, {previous.Item}");
                            OnContentChange(ListeningChangeEventArgs.From(previous, Live));
                            OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                        }
                        // PODCASTS
                        else if(previous.Item is FullEpisode previousEp 
                            && Live.Item is FullEpisode currentEp)
                        {
                            if(!eq.IsEqual(previousEp, currentEp)) {
                                Logger.LogDebug($"Podcast changed: {previousEp.DisplayString()} -> {currentEp.DisplayString()}");
                                OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                            }
                        }
                        else {
                            throw new NotSupportedException("Unknown item combination");
                        }

                        // CONTEXT
                        if(!eq.IsEqual(previous.Context, Live.Context)) {
                            Logger.LogDebug($"Context changed: {previous.Context.DisplayString()} -> {Live.Context.DisplayString()}");
                            OnContextChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // DEVICE
                        if(!eq.IsEqual(previous?.Device, Live?.Device)) {
                            Logger.LogDebug($"Device changed: {previous?.Device.DisplayString()} -> {Live?.Device.DisplayString()}");
                            OnDeviceChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // IS PLAYING
                        if(previous.IsPlaying != Live.IsPlaying) {
                            Logger.LogDebug($"Playing state changed: {previous.IsPlaying} -> {Live.IsPlaying}");
                            OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // VOLUME
                        if(previous.Device.VolumePercent != Live.Device.VolumePercent) {
                            Logger.LogDebug($"Volume changed: {previous.Device.VolumePercent}% -> {Live.Device.VolumePercent}%");
                            OnVolumeChange(ListeningChangeEventArgs.From(previous, Live));
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
                throw e;
            }
            catch(APIException e)
            {
                Logger.LogDebug($"API error: [{e.Message}]");
                throw e;
            }
        }

        /// <summary>
        /// Store currently playing in last plays. Determine whether new list or appending required
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreCurrentPlaying(CurrentlyPlayingContext current) 
        {
            Past?.Add(current);
        }

        #region Event Firers
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
