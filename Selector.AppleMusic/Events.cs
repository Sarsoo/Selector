using Selector.AppleMusic.Watcher;

namespace Selector.AppleMusic;

public class AppleListeningChangeEventArgs : EventArgs
{
    public AppleMusicCurrentlyPlayingContext Previous { get; set; }
    public AppleMusicCurrentlyPlayingContext Current { get; set; }

    /// <summary>
    /// String Id for watcher, used to hold user Db Id
    /// </summary>
    /// <value></value>
    public string Id { get; set; }
    // AppleTimeline Timeline { get; set; }

    public static AppleListeningChangeEventArgs From(AppleMusicCurrentlyPlayingContext previous,
        AppleMusicCurrentlyPlayingContext current, AppleTimeline timeline, string id = null, string username = null)
    {
        return new AppleListeningChangeEventArgs()
        {
            Previous = previous,
            Current = current,
            // Timeline = timeline,
            Id = id
        };
    }
}