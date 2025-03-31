using SpotifyAPI.Web;

namespace Selector.Spotify
{
    public interface IPlaylistWatcher : IWatcher
    {
        public event EventHandler<PlaylistChangeEventArgs> NetworkPoll;
        public event EventHandler<PlaylistChangeEventArgs> SnapshotChange;

        public event EventHandler<PlaylistChangeEventArgs> TracksAdded;
        public event EventHandler<PlaylistChangeEventArgs> TracksRemoved;

        public event EventHandler<PlaylistChangeEventArgs> NameChanged;
        public event EventHandler<PlaylistChangeEventArgs> DescriptionChanged;

        public FullPlaylist Live { get; }
        public Timeline<FullPlaylist> Past { get; }
    }
}