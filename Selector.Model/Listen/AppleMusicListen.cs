using Selector.AppleMusic.Watcher;

namespace Selector.Model;

public class AppleMusicListen : Listen, IUserListen
{
    public int Id { get; set; }

    public string TrackId { get; set; }
    public string Isrc { get; set; }
    public bool IsScrobbled { get; set; }
    public bool ScrobbleIgnored { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public static explicit operator AppleMusicListen(AppleMusicCurrentlyPlayingContext track) => new()
    {
        Timestamp = track.FirstSeen,

        TrackId = track.Track.Id,
        Isrc = track.Track.Attributes.Isrc,
        IsScrobbled = track.Scrobbled,
        ScrobbleIgnored = track.ScrobbleIgnored,

        TrackName = track.Track.Attributes.Name,
        AlbumName = track.Track.Attributes.AlbumName,
        ArtistName = track.Track.Attributes.ArtistName,
    };
}