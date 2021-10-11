using SpotifyAPI.Web;

namespace Selector
{
    public class StringEqual: Equal
    {
        public StringEqual()
        {
            comps = new()
            {
                { typeof(FullTrack), new FullTrackStringComparer() },
                { typeof(FullEpisode), new FullEpisodeStringComparer() },
                { typeof(FullAlbum), new FullAlbumStringComparer() },
                { typeof(FullShow), new FullShowStringComparer() },
                { typeof(FullArtist), new FullArtistStringComparer() },

                { typeof(SimpleTrack), new SimpleTrackStringComparer() },
                { typeof(SimpleEpisode), new SimpleEpisodeStringComparer() },
                { typeof(SimpleAlbum), new SimpleAlbumStringComparer() },
                { typeof(SimpleShow), new SimpleShowStringComparer() },
                { typeof(SimpleArtist), new SimpleArtistStringComparer() },
            };
        }
    }
}
