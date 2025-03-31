using System.Collections.Generic;
using FluentAssertions;
using Selector.Spotify.Equality;
using SpotifyAPI.Web;
using Xunit;

namespace Selector.Tests
{
    public class EqualTests
    {
        public static IEnumerable<object[]> TrackData =>
            new List<object[]>
            {
                // SAME
                new object[]
                {
                    Helper.FullTrack("1", "1", "1"),
                    Helper.FullTrack("1", "1", "1"),
                    true
                },
                // WRONG ALBUM
                new object[]
                {
                    Helper.FullTrack("1", "1", "1"),
                    Helper.FullTrack("1", "2", "1"),
                    false
                },
                // WRONG TRACK
                new object[]
                {
                    Helper.FullTrack("1", "1", "1"),
                    Helper.FullTrack("2", "1", "1"),
                    false
                },
                // WRONG ARTIST
                new object[]
                {
                    Helper.FullTrack("1", "1", "1"),
                    Helper.FullTrack("1", "1", "2"),
                    false
                },
                // WRONG TRACK/ARTIST
                new object[]
                {
                    Helper.FullTrack("1", "1", "1"),
                    Helper.FullTrack("2", "1", "2"),
                    false
                },
                // RIGHT MULTIPLE ARTISTS
                new object[]
                {
                    Helper.FullTrack("1", "1", new List<string>() { "1", "2" }),
                    Helper.FullTrack("1", "1", new List<string>() { "1", "2" }),
                    true
                },
                // WRONG ARTISTS
                new object[]
                {
                    Helper.FullTrack("1", "1", new List<string>() { "1", "2" }),
                    Helper.FullTrack("1", "1", new List<string>() { "1" }),
                    false
                }
            };

        [Theory]
        [MemberData(nameof(TrackData))]
        public void TrackEquality(FullTrack track1, FullTrack track2, bool shouldEqual)
        {
            var eq = new StringEqual();
            eq.IsEqual(track1, track2).Should().Be(shouldEqual);
        }

        public static IEnumerable<object[]> AlbumData =>
            new List<object[]>
            {
                // SAME
                new object[]
                {
                    Helper.SimpleAlbum("1", "1"),
                    Helper.SimpleAlbum("1", "1"),
                    true
                },
                // DIFFERENT NAME
                new object[]
                {
                    Helper.SimpleAlbum("1", "1"),
                    Helper.SimpleAlbum("2", "1"),
                    false
                },
                // DIFFERENT ARTIST
                new object[]
                {
                    Helper.SimpleAlbum("1", "1"),
                    Helper.SimpleAlbum("1", "2"),
                    false
                },
                // SAME ARTISTS
                new object[]
                {
                    Helper.SimpleAlbum("1", new List<string>() { "1", "2" }),
                    Helper.SimpleAlbum("1", new List<string>() { "1", "2" }),
                    true
                },
                // DIFFERENT ARTISTS
                new object[]
                {
                    Helper.SimpleAlbum("1", new List<string>() { "1", "2" }),
                    Helper.SimpleAlbum("1", new List<string>() { "1" }),
                    false
                },
            };

        [Theory]
        [MemberData(nameof(AlbumData))]
        public void AlbumEquality(SimpleAlbum album1, SimpleAlbum album2, bool shouldEqual)
        {
            var eq = new StringEqual();
            eq.IsEqual(album1, album2).Should().Be(shouldEqual);
        }

        public static IEnumerable<object[]> ArtistData =>
            new List<object[]>
            {
                // SAME
                new object[]
                {
                    Helper.SimpleArtist("1"),
                    Helper.SimpleArtist("1"),
                    true
                },
                // DIFFERENT
                new object[]
                {
                    Helper.SimpleArtist("1"),
                    Helper.SimpleArtist("2"),
                    false
                }
            };

        [Theory]
        [MemberData(nameof(ArtistData))]
        public void ArtistEquality(SimpleArtist artist1, SimpleArtist artist2, bool shouldEqual)
        {
            var eq = new StringEqual();
            eq.IsEqual(artist1, artist2).Should().Be(shouldEqual);
        }

        public static IEnumerable<object[]> EpisodeData =>
            new List<object[]>
            {
                // SAME
                new object[]
                {
                    Helper.FullEpisode("1"),
                    Helper.FullEpisode("1"),
                    true
                },
                // DIFFERENT
                new object[]
                {
                    Helper.FullEpisode("1"),
                    Helper.FullEpisode("2"),
                    false
                }
            };

        [Theory]
        [MemberData(nameof(EpisodeData))]
        public void EpisodeEquality(FullEpisode episode1, FullEpisode episode2, bool shouldEqual)
        {
            var eq = new StringEqual();
            eq.IsEqual(episode1, episode2).Should().Be(shouldEqual);
        }
    }
}