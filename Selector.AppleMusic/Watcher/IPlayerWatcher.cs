using Selector.AppleMusic;
using Selector.AppleMusic.Watcher;

namespace Selector
{
    public interface IAppleMusicPlayerWatcher : IWatcher<AppleListeningChangeEventArgs>
    {
        public event EventHandler<AppleListeningChangeEventArgs> NetworkPoll;
        public event EventHandler<AppleListeningChangeEventArgs> AlbumChange;
        public event EventHandler<AppleListeningChangeEventArgs> ArtistChange;

        /// <summary>
        /// Last retrieved currently playing
        /// </summary>
        public AppleMusicCurrentlyPlayingContext? Live { get; }

        public AppleTimeline Past { get; }
    }
}