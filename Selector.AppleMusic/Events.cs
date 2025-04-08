using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic;

public class AppleListeningChangeEventArgs : ListeningChangeEventArgs
{
    public required AppleMusicCurrentlyPlayingContext? Previous { get; set; }
    public required AppleMusicCurrentlyPlayingContext? Current { get; set; }

    public required AppleTimeline Timeline { get; set; }

    public static AppleListeningChangeEventArgs From(AppleMusicCurrentlyPlayingContext? previous,
        AppleMusicCurrentlyPlayingContext? current, AppleTimeline timeline, string id, string? username = null)
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