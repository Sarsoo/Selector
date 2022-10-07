using System;

namespace Selector
{
    public class Scrobble: IListen
    {
        public DateTime Timestamp { get; set; }
        public string TrackName { get; set; }
        public string AlbumName { get; set; }
        /// <summary>
        /// Not populated by default from the service, where not the same as <see cref="ArtistName"/> these have been manually entered
        /// </summary>
        public string AlbumArtistName { get; set; }
        public string ArtistName { get; set; }
        
        public static explicit operator Scrobble(IF.Lastfm.Core.Objects.LastTrack track) => new()
        {
            Timestamp = track.TimePlayed?.UtcDateTime ?? DateTime.MinValue,

            TrackName = track.Name,
            AlbumName = track.AlbumName,
            ArtistName = track.ArtistName,
        };

        public override string ToString() => $"({Timestamp}) {TrackName}, {AlbumName}, {ArtistName}";
    }
}
