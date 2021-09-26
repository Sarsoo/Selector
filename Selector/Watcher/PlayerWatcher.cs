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
        private IEqualityChecker equalityChecker;

        public event EventHandler<ListeningChangeEventArgs> ItemChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;
        public event EventHandler<ListeningChangeEventArgs> ContentChange;

        // public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        // public event EventHandler<ListeningChangeEventArgs> DeviceChange;
        public event EventHandler<ListeningChangeEventArgs> PlayingChange;

        private CurrentlyPlaying live { get; set; }
        private List<List<CurrentlyPlaying>> lastPlays { get; set; }

        private int _pollPeriod;
        public int PollPeriod {
            get => _pollPeriod;
            set => _pollPeriod = Math.Max(0, value);
        }

        public PlayerWatcher(IPlayerClient spotifyClient, 
                IEqualityChecker equalityChecker,
                int pollPeriod = 3000) {

            this.spotifyClient = spotifyClient;
            this.equalityChecker = equalityChecker;
            this.PollPeriod = pollPeriod;

            lastPlays = new List<List<CurrentlyPlaying>>();
        }

        public async Task WatchOne() 
        {
            try{
                var polledCurrent = await spotifyClient.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());

                if (polledCurrent != null) StoreCurrentPlaying(polledCurrent);

                CurrentlyPlaying previous;
                if(live is null) {
                    live = polledCurrent;
                    previous = polledCurrent;
                }
                else {
                    previous = live;
                    live = polledCurrent;
                }

                // NOT PLAYING
                if(previous is null && live is null)
                {
                    // Console.WriteLine("not playing");
                }
                else
                {
                    // STARTED PLAYBACK
                    if(previous is null && (live.Item is FullTrack || live.Item is FullEpisode))
                    {
                        // Console.WriteLine("started playing");

                    }
                    // STOPPED PLAYBACK
                    else if((previous.Item is FullTrack || previous.Item is FullEpisode) && live is null)
                    {
                        // Console.WriteLine("stopped playing");

                    }
                    else {

                        // MUSIC
                        if(previous.Item is FullTrack && live.Item is FullTrack)
                        {
                            var previousItem = (FullTrack) previous.Item;
                            var currentItem = (FullTrack) live.Item;

                            if(!equalityChecker.Track(previousItem, currentItem, true)) {
                                OnItemChange(new ListeningChangeEventArgs(){
                                    Previous = previous,
                                    Current = live
                                });
                            }

                            if(!equalityChecker.Album(previousItem.Album, currentItem.Album)) {
                                OnAlbumChange(new ListeningChangeEventArgs(){
                                    Previous = previous,
                                    Current = live
                                });
                            }

                            if(!equalityChecker.Artist(previousItem.Artists[0], currentItem.Artists[0])) {
                                OnArtistChange(new ListeningChangeEventArgs(){
                                    Previous = previous,
                                    Current = live
                                });
                            }
                        }
                        // CHANGED CONTENT
                        else if(previous.Item is FullTrack && live.Item is FullEpisode
                            || previous.Item is FullEpisode && live.Item is FullTrack)
                        {
                            OnContentChange(new ListeningChangeEventArgs(){
                                Previous = previous,
                                Current = live
                            });
                            OnItemChange(new ListeningChangeEventArgs(){
                                Previous = previous,
                                Current = live
                            });
                        }
                        // PODCASTS
                        else if(previous.Item is FullEpisode && live.Item is FullEpisode)
                        {
                            var previousItem = (FullEpisode) previous.Item;
                            var currentItem = (FullEpisode) live.Item;

                            if(!equalityChecker.Episode(previousItem, currentItem)) {
                                OnItemChange(new ListeningChangeEventArgs(){
                                    Previous = previous,
                                    Current = live
                                });
                            }
                        }
                        else {
                            throw new NotSupportedException("Unknown item combination");
                        }

                        // CONTEXT
                        if(!equalityChecker.Context(previous.Context, live.Context)) {
                            OnContextChange(new ListeningChangeEventArgs(){
                                Previous = previous,
                                Current = live
                            });
                        }

                        // IS PLAYING
                        if(previous.IsPlaying != live.IsPlaying) {
                            OnPlayingChange(new ListeningChangeEventArgs(){
                                Previous = previous,
                                Current = live
                            });
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

        public async Task Watch(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await WatchOne();
                await Task.Delay(PollPeriod);
            }
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
