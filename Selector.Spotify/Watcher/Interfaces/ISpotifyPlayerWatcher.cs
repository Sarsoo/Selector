using Selector.Spotify.Timeline;
using SpotifyAPI.Web;

namespace Selector.Spotify
{
    public interface ISpotifyPlayerWatcher : IWatcher
    {
        /// <summary>
        /// Track or episode changes
        /// </summary>
        public event EventHandler<SpotifyListeningChangeEventArgs> NetworkPoll;

        public event EventHandler<SpotifyListeningChangeEventArgs> ItemChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> AlbumChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> ArtistChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> ContextChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> ContentChange;

        public event EventHandler<SpotifyListeningChangeEventArgs> VolumeChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> DeviceChange;
        public event EventHandler<SpotifyListeningChangeEventArgs> PlayingChange;

        /// <summary>
        /// Last retrieved currently playing
        /// </summary>
        public CurrentlyPlayingContext Live { get; }

        public PlayerTimeline Past { get; }
    }
}