using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Selector
{
    public class PlayerWatcher: IPlayerWatcher
    {
        private readonly IPlayerClient spotifyClient;
        private IScheduler sleepScheduler;
        private IEqualityChecker equalityChecker;

        public event EventHandler<ListeningChangeEventArgs> TrackChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;

        // public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        // public event EventHandler<ListeningChangeEventArgs> DeviceChange;
        public event EventHandler<ListeningChangeEventArgs> PlayingChange;

        private CurrentlyPlaying live { get; set; }
        private List<List<CurrentlyPlaying>> lastPlays { get; set; }

        public PlayerWatcher(IPlayerClient spotifyClient, IScheduler sleepScheduler, IEqualityChecker equalityChecker) {
            this.spotifyClient = spotifyClient;
            this.sleepScheduler = sleepScheduler;
            this.equalityChecker = equalityChecker;

            lastPlays = new List<List<CurrentlyPlaying>>();
        }

        public async Task WatchOne() 
        {
            var polledCurrent = await spotifyClient.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

            StoreCurrentPlaying(polledCurrent);

            CurrentlyPlaying existing;
            if(live is null) {
                live = polledCurrent;
                existing = polledCurrent;
            }
            else {
                existing = live;
                live = polledCurrent;
            }
            
            try{
                var existingItem = (FullTrack) existing.Item;
                var currentItem = (FullTrack) live.Item;

                if(!equalityChecker.Track(existingItem, currentItem, true)) {
                    OnTrackChange(new ListeningChangeEventArgs(){
                        Previous = existing,
                        Current = live
                    });
                }

                if(!equalityChecker.Album(existingItem.Album, currentItem.Album)) {
                    OnAlbumChange(new ListeningChangeEventArgs(){
                        Previous = existing,
                        Current = live
                    });
                }

                if(!equalityChecker.Artist(existingItem.Artists[0], currentItem.Artists[0])) {
                    OnArtistChange(new ListeningChangeEventArgs(){
                        Previous = existing,
                        Current = live
                    });
                }

                if(!equalityChecker.Context(existing.Context, live.Context)) {
                    OnContextChange(new ListeningChangeEventArgs(){
                        Previous = existing,
                        Current = live
                    });
                }

                if(existing.IsPlaying != live.IsPlaying) {
                    OnPlayingChange(new ListeningChangeEventArgs(){
                        Previous = existing,
                        Current = live
                    });
                }
            }
            catch(InvalidCastException)
            {
                var existingItem = (FullEpisode) existing.Item;

                throw new NotImplementedException("Podcasts not implemented");
            }
        }

        public Task Watch(CancellationToken cancelToken)
        {
            return Task.CompletedTask;
        }

        public CurrentlyPlaying NowPlaying()
        {
            return live;
        }

        /// <summary>
        /// Store currently playing in last plays. Determine whether new list or appending required
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreCurrentPlaying(CurrentlyPlaying current) 
        {
            if(lastPlays.Count > 0)
            {
                bool matchesMostRecent;

                try {
                    var castItem = (FullTrack) current.Item;
                    var castStoredItem = (FullTrack) lastPlays[0][0].Item;

                    matchesMostRecent = equalityChecker.Track(castItem, castStoredItem, true);
                }
                catch(InvalidCastException)
                {
                    var castItem = (FullEpisode) current.Item;
                    var castStoredItem = (FullEpisode) lastPlays[0][0].Item;

                    matchesMostRecent = equalityChecker.Episode(castItem, castStoredItem);
                }

                if (matchesMostRecent)
                {
                    lastPlays[0].Add(current);
                }
                else 
                {
                    StoreNewTrack(current);
                }
            }
            else {
                StoreNewTrack(current);
            }
        }

        /// <summary>
        /// Store currently playing at front of last plays list. Pushes new list to hold same track
        /// </summary>
        /// <param name="current">New currently playing to store</param>
        private void StoreNewTrack(CurrentlyPlaying current)
        {
            if (live != null) {
                var newPlayingList = new List<CurrentlyPlaying>();
                newPlayingList.Add(live);
                lastPlays.Insert(0, newPlayingList);
            }
        }

        protected virtual void OnTrackChange(ListeningChangeEventArgs args)
        {
            TrackChange?.Invoke(this, args); 
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


        // protected virtual void OnVolumeChange(ListeningChangeEventArgs args)
        // {
        //     ArtistChange?.Invoke(this, args);
        // }

        // protected virtual void OnDeviceChange(ListeningChangeEventArgs args)
        // {
        //     ContextChange?.Invoke(this, args); 
        // }

        protected virtual void OnPlayingChange(ListeningChangeEventArgs args)
        {
            PlayingChange?.Invoke(this, args); 
        }
    }
}
