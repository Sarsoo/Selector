using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic;

public class AppleListeningChangeEventArgs : ListeningChangeEventArgs
{
    public AppleMusicCurrentlyPlayingContext Previous { get; set; }
    public AppleMusicCurrentlyPlayingContext Current { get; set; }

    AppleTimeline Timeline { get; set; }

    public static AppleListeningChangeEventArgs From(AppleMusicCurrentlyPlayingContext previous,
        AppleMusicCurrentlyPlayingContext current, AppleTimeline timeline, string id = null, string username = null)
    {
        return new AppleListeningChangeEventArgs()
        {
            Previous = previous,
            Current = current,
            Timeline = timeline,
            Id = id
        };
    }
}