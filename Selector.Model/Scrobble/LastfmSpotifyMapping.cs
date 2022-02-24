namespace Selector.Model
{
    public abstract class LastfmSpotifyMapping
    {
        public string SpotifyUri { get; set; }
    }

    public class TrackLastfmSpotifyMapping: LastfmSpotifyMapping
    {
        public string LastfmTrackName { get; set; }
        public string LastfmArtistName { get; set; }
    }

    public class AlbumLastfmSpotifyMapping : LastfmSpotifyMapping
    {
        public string LastfmAlbumName { get; set; }
        public string LastfmArtistName { get; set; }
    }

    public class ArtistLastfmSpotifyMapping : LastfmSpotifyMapping
    {
        public string LastfmArtistName { get; set; }
    }
}
