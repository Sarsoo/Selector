using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;
using SpotifyAPI.Web;

using Selector;

namespace Selector.Tests
{
    public class UriEqualTests
    {
        public static IEnumerable<object[]> TrackData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.FullTrack("1"),
                Helper.FullTrack("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.FullTrack("1"),
                Helper.FullTrack("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(TrackData))]
        public void TrackEquality(FullTrack track1, FullTrack track2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<FullTrack>(track1, track2)
                .Should()
                .Be(shouldEqual);
        }

        public static IEnumerable<object[]> AlbumData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.SimpleAlbum("1"),
                Helper.SimpleAlbum("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.SimpleAlbum("1"),
                Helper.SimpleAlbum("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(AlbumData))]
        public void AlbumEquality(SimpleAlbum album1, SimpleAlbum album2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<SimpleAlbum>(album1, album2)
                .Should()
                .Be(shouldEqual);
        }

        public static IEnumerable<object[]> ArtistData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.SimpleArtist("1"),
                Helper.SimpleArtist("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.SimpleArtist("1"),
                Helper.SimpleArtist("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(ArtistData))]
        public void ArtistEquality(SimpleArtist artist1, SimpleArtist artist2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<SimpleArtist>(artist1, artist2)
                .Should()
                .Be(shouldEqual);
        }

        public static IEnumerable<object[]> EpisodeData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.FullEpisode("1"),
                Helper.FullEpisode("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.FullEpisode("1"),
                Helper.FullEpisode("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(EpisodeData))]
        public void EpisodeEquality(FullEpisode ep1, FullEpisode ep2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<FullEpisode>(ep1, ep2)
                .Should()
                .Be(shouldEqual);
        }

        public static IEnumerable<object[]> ContextData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.Context("1"),
                Helper.Context("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.Context("1"),
                Helper.Context("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(ContextData))]
        public void ContextEquality(Context context1, Context context2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<Context>(context1, context2)
                .Should()
                .Be(shouldEqual);
        }

        public static IEnumerable<object[]> DeviceData => 
        new List<object[]>
        {
            // SAME
            new object[] {
                Helper.Device("1"),
                Helper.Device("1"),
                true
            },
            // WRONG
            new object[] {
                Helper.Device("1"),
                Helper.Device("2"),
                false
            }
        };

        [Theory]
        [MemberData(nameof(DeviceData))]
        public void DeviceEquality(Device device1, Device device2, bool shouldEqual)
        {
            new UriEqual()
                .IsEqual<Device>(device1, device2)
                .Should()
                .Be(shouldEqual);
        }
    }
}
