using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Selector
{
    public class PlayerWatcher: BaseWatcher, IPlayerWatcher
    {
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
        public ITimeline<CurrentlyPlayingContext> Past { get; private set; }
        private List<List<CurrentlyPlayingContext>> lastPlays { get; set; }

        public PlayerWatcher(IPlayerClient spotifyClient, 
                IEqual equalityChecker,
                int pollPeriod = 3000) {

            this.spotifyClient = spotifyClient;
            this.eq = equalityChecker;
            this.PollPeriod = pollPeriod;

            lastPlays = new List<List<CurrentlyPlayingContext>>();
        }

        public override async Task WatchOne() 
        {
            try{
                var polledCurrent = await spotifyClient.GetCurrentPlayback();

                if (polledCurrent != null) StoreCurrentPlaying(Live, polledCurrent);

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
                    if(previous is null && (Live.Item is FullTrack || Live.Item is FullEpisode))
                    {
                        OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                    }
                    // STOPPED PLAYBACK
                    else if((previous.Item is FullTrack || previous.Item is FullEpisode) && Live is null)
                    {
                        OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                    }
                    // CONTINUING PLAYBACK
                    else {

                        // MUSIC
                        if(previous.Item is FullTrack previousTrack && Live.Item is FullTrack currentTrack)
                        {

                            if(!eq.IsEqual<FullTrack>(previousTrack, currentTrack)) {
                                OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                            }

                            if(!eq.IsEqual<SimpleAlbum>(previousTrack.Album, currentTrack.Album)) {
                                OnAlbumChange(ListeningChangeEventArgs.From(previous, Live));
                            }

                            if(!eq.IsEqual<SimpleArtist>(previousTrack.Artists[0], currentTrack.Artists[0])) {
                                OnArtistChange(ListeningChangeEventArgs.From(previous, Live));
                            }
                        }
                        // CHANGED CONTENT
                        else if(previous.Item is FullTrack && Live.Item is FullEpisode
                            || previous.Item is FullEpisode && Live.Item is FullTrack)
                        {
                            OnContentChange(ListeningChangeEventArgs.From(previous, Live));
                            OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                        }
                        // PODCASTS
                        else if(previous.Item is FullEpisode previousEp && Live.Item is FullEpisode currentEp)
                        {
                            if(!eq.IsEqual<FullEpisode>(previousEp, currentEp)) {
                                OnItemChange(ListeningChangeEventArgs.From(previous, Live));
                            }
                        }
                        else {
                            throw new NotSupportedException("Unknown item combination");
                        }

                        // CONTEXT
                        if(!eq.IsEqual<Context>(previous.Context, Live.Context)) {
                            OnContextChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // DEVICE
                        if(!eq.IsEqual<Device>(previous?.Device, Live?.Device)) {
                            OnDeviceChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // IS PLAYING
                        if(previous.IsPlaying != Live.IsPlaying) {
                            OnPlayingChange(ListeningChangeEventArgs.From(previous, Live));
                        }

                        // VOLUME
                        if(previous.Device.VolumePercent != Live.Device.VolumePercent) {
                            OnVolumeChange(ListeningChangeEventArgs.From(previous, Live));
                        }
                    }
                }                
            }
            catch(APIUnauthorizedException e)
            {
                throw e;
            }
            catch(APITooManyRequestsException e)
            {
                throw e;
            }
            catch(APIException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Store currently playing in last plays. Determine whether new list or appending required
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreCurrentPlaying(CurrentlyPlayingContext previous, CurrentlyPlayingContext current) 
        {
            if(lastPlays.Count > 0)
            {
                bool matchesMostRecent;

                try {
                    var castItem = (FullTrack) current.Item;
                    var castStoredItem = (FullTrack) lastPlays[0][0].Item;

                    matchesMostRecent = eq.IsEqual<FullTrack>(castItem, castStoredItem);
                }
                catch(InvalidCastException)
                {
                    var castItem = (FullEpisode) current.Item;
                    var castStoredItem = (FullEpisode) lastPlays[0][0].Item;

                    matchesMostRecent = eq.IsEqual<FullEpisode>(castItem, castStoredItem);
                }

                if (matchesMostRecent)
                {
                    lastPlays[0].Add(current);
                }
                else 
                {
                    StoreNewTrack(previous, current);
                }
            }
            else {
                StoreNewTrack(previous, current);
            }
        }

        /// <summary>
        /// Store currently playing at front of last plays list. Pushes new list to hold same track
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreNewTrack(CurrentlyPlayingContext previous, CurrentlyPlayingContext current)
        {
            if (previous != null) {
                var newPlayingList = new List<CurrentlyPlayingContext>();
                newPlayingList.Add(previous);
                lastPlays.Insert(0, newPlayingList);
            }
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
    }
}
