using Selector.AppleMusic;
using Selector.AppleMusic.Watcher;

namespace Selector
{
    public interface IAppleMusicPlayerWatcher : IWatcher
    {
        public event EventHandler<AppleListeningChangeEventArgs> NetworkPoll;
        public event EventHandler<AppleListeningChangeEventArgs> ItemChange;
        public event EventHandler<AppleListeningChangeEventArgs> AlbumChange;
        public event EventHandler<AppleListeningChangeEventArgs> ArtistChange;

        /// <summary>
        /// Last retrieved currently playing
        /// </summary>
        public AppleMusicCurrentlyPlayingContext Live { get; }

        public AppleTimeline Past { get; }
    }
}