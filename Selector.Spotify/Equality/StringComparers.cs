using SpotifyAPI.Web;

namespace Selector.Spotify.Equality
{
    public abstract class NoHashCode<T> : EqualityComparer<T>
    {
        public override int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }

    public class FullTrackStringComparer : NoHashCode<FullTrack>
    {
        public override bool Equals(FullTrack track1, FullTrack track2) => IsEqual(track1, track2);

        public static bool IsEqual(FullTrack track1, FullTrack track2) => track1.Name == track2.Name
                                                                          && Enumerable.SequenceEqual(
                                                                              track1.Artists.Select(a => a.Name),
                                                                              track2.Artists.Select(a => a.Name))
                                                                          && SimpleAlbumStringComparer.IsEqual(
                                                                              track1.Album, track2.Album);
    }

    public class FullEpisodeStringComparer : NoHashCode<FullEpisode>
    {
        public override bool Equals(FullEpisode ep1, FullEpisode ep2) => IsEqual(ep1, ep2);

        public static bool IsEqual(FullEpisode ep1, FullEpisode ep2) => ep1.Name == ep2.Name
                                                                        && SimpleShowStringComparer.IsEqual(ep1.Show,
                                                                            ep2.Show);
    }

    public class FullAlbumStringComparer : NoHashCode<FullAlbum>
    {
        public override bool Equals(FullAlbum album1, FullAlbum album2) => IsEqual(album1, album2);

        public static bool IsEqual(FullAlbum album1, FullAlbum album2) => album1.Name == album2.Name
                                                                          && Enumerable.SequenceEqual(
                                                                              album1.Artists.Select(a => a.Name),
                                                                              album2.Artists.Select(a => a.Name));
    }

    public class FullShowStringComparer : NoHashCode<FullShow>
    {
        public override bool Equals(FullShow show1, FullShow show2) => IsEqual(show1, show2);

        public static bool IsEqual(FullShow show1, FullShow show2) => show1.Name == show2.Name
                                                                      && show1.Publisher == show2.Publisher;
    }

    public class FullArtistStringComparer : NoHashCode<FullArtist>
    {
        public override bool Equals(FullArtist artist1, FullArtist artist2) => IsEqual(artist1, artist2);

        public static bool IsEqual(FullArtist artist1, FullArtist artist2) => artist1.Name == artist2.Name;
    }

    public class SimpleTrackStringComparer : NoHashCode<SimpleTrack>
    {
        public override bool Equals(SimpleTrack track1, SimpleTrack track2) => IsEqual(track1, track2);

        public static bool IsEqual(SimpleTrack track1, SimpleTrack track2) => track1.Name == track2.Name
                                                                              && Enumerable.SequenceEqual(
                                                                                  track1.Artists.Select(a => a.Name),
                                                                                  track2.Artists.Select(a => a.Name));
    }

    public class SimpleEpisodeStringComparer : NoHashCode<SimpleEpisode>
    {
        public override bool Equals(SimpleEpisode ep1, SimpleEpisode ep2) => IsEqual(ep1, ep2);

        public static bool IsEqual(SimpleEpisode ep1, SimpleEpisode ep2) => ep1.Name == ep2.Name;
    }

    public class SimpleAlbumStringComparer : NoHashCode<SimpleAlbum>
    {
        public override bool Equals(SimpleAlbum album1, SimpleAlbum album2) => IsEqual(album1, album2);

        public static bool IsEqual(SimpleAlbum album1, SimpleAlbum album2) => album1.Name == album2.Name
                                                                              && Enumerable.SequenceEqual(
                                                                                  album1.Artists.Select(a => a.Name),
                                                                                  album2.Artists.Select(a => a.Name));
    }

    public class SimpleShowStringComparer : NoHashCode<SimpleShow>
    {
        public override bool Equals(SimpleShow show1, SimpleShow show2) => IsEqual(show1, show2);

        public static bool IsEqual(SimpleShow show1, SimpleShow show2) => show1.Name == show2.Name
                                                                          && show1.Publisher == show2.Publisher;
    }

    public class SimpleArtistStringComparer : NoHashCode<SimpleArtist>
    {
        public override bool Equals(SimpleArtist artist1, SimpleArtist artist2) => IsEqual(artist1, artist2);

        public static bool IsEqual(SimpleArtist artist1, SimpleArtist artist2) => artist1.Name == artist2.Name;
    }
}