using Selector.AppleMusic.Model;

namespace Selector.AppleMusic.Watcher;

public class AppleMusicCurrentlyPlayingContext
{
    public DateTime FirstSeen { get; set; }
    public required Track Track { get; set; }
    public bool Scrobbled { get; set; }
    public bool ScrobbleIgnored { get; set; }

    public bool ToScrobble => !Scrobbled && !ScrobbleIgnored;
}

public class AppleMusicCurrentlyPlayingContextComparer : IEqualityComparer<AppleMusicCurrentlyPlayingContext>
{
    public bool Equals(AppleMusicCurrentlyPlayingContext? x, AppleMusicCurrentlyPlayingContext? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Track.Id.Equals(y.Track.Id);
    }

    public int GetHashCode(AppleMusicCurrentlyPlayingContext obj)
    {
        return obj.Track.GetHashCode();
    }
}