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

        public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        public event EventHandler<ListeningChangeEventArgs> DeviceChange;

        private CurrentlyPlaying live { get; set; }
        //public List<CurrentlyPlaying> LastPlays { get; private set; }

        public PlayerWatcher(IPlayerClient spotifyClient, IScheduler sleepScheduler, IEqualityChecker equalityChecker) {
            this.spotifyClient = spotifyClient;
            this.sleepScheduler = sleepScheduler;
            this.equalityChecker = equalityChecker;
        }

        public async Task WatchOne() 
        {
            
        }

        public Task Watch(CancellationToken cancelToken)
        {
            return Task.CompletedTask;
        }

        public CurrentlyPlaying NowPlaying()
        {
            throw new NotImplementedException();
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


        protected virtual void OnVolumeChange(ListeningChangeEventArgs args)
        {
            ArtistChange?.Invoke(this, args);
        }

        protected virtual void OnDeviceChange(ListeningChangeEventArgs args)
        {
            ContextChange?.Invoke(this, args); 
        }
    }
}
