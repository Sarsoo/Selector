using System;
using System.Collections.Generic;
using SpotifyAPI.Web;

namespace Selector
{
    public interface IPlayerWatcher: IWatcher
    {
        public event EventHandler<ListeningChangeEventArgs> TrackChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;

        public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        public event EventHandler<ListeningChangeEventArgs> DeviceChange;

        public CurrentlyPlaying NowPlaying();
        // recently playing
    }
}
