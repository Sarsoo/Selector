using System;
using SpotifyAPI.Web;

namespace Selector
{
    public interface ISpotifyPlayerWatcher : IWatcher
    {
        /// <summary>
        /// Track or episode changes
        /// </summary>
        public event EventHandler<ListeningChangeEventArgs> NetworkPoll;

        public event EventHandler<ListeningChangeEventArgs> ItemChange;
        public event EventHandler<ListeningChangeEventArgs> AlbumChange;
        public event EventHandler<ListeningChangeEventArgs> ArtistChange;
        public event EventHandler<ListeningChangeEventArgs> ContextChange;
        public event EventHandler<ListeningChangeEventArgs> ContentChange;

        public event EventHandler<ListeningChangeEventArgs> VolumeChange;
        public event EventHandler<ListeningChangeEventArgs> DeviceChange;
        public event EventHandler<ListeningChangeEventArgs> PlayingChange;

        /// <summary>
        /// Last retrieved currently playing
        /// </summary>
        public CurrentlyPlayingContext Live { get; }

        public PlayerTimeline Past { get; }
    }
}