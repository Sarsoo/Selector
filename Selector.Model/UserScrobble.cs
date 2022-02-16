using System;

namespace Selector.Model
{
    public class UserScrobble: Scrobble
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public static explicit operator UserScrobble(IF.Lastfm.Core.Objects.LastTrack track) => new()
        {
            Timestamp = track.TimePlayed?.UtcDateTime ?? DateTime.MinValue,

            TrackName = track.Name,
            AlbumName = track.AlbumName,
            ArtistName = track.ArtistName,
        };

    }
}
