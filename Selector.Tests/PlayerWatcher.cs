using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;
using SpotifyAPI.Web;

using Selector;

namespace Selector.Tests
{
    public class PlayerWatcherTests
    {
        //public static IEnumerable<object[]> TrackData => 
        //new List<object[]>
        //{
        //    new object[] { new List<CurrentlyPlaying>(){
        //        new CurrentlyPlaying(){
        //            Item = new FullTrack() {

        //            }
        //        }
        //    }, 1 }
        //};

        //[Theory]
        //[MemberData(nameof(TrackData))]
        //public void Test1(List<CurrentlyPlaying> playing)
        //{
        //    var spotMock = new Mock<IPlayerClient>();
        //    // spotMock.Setup(spot => spot.GetCurrentlyPlaying(It.IsAny<PlayerCurrentlyPlayingRequest>())).Returns();
        //    // var watch = new Watcher();
        //}

        // [Fact]
        // public void Test2()
        // {
        //     var artist = new SimpleArtist(){
        //                 Name = "test"
        //             };
        //     var track = new FullTrack() {
        //         Name = "Test",
        //         Album = new SimpleAlbum() {
        //             Name = "test",
        //             Artists = new List<SimpleArtist>(){
        //                 artist
        //             }
        //         },
        //         Artists = new List<SimpleArtist>(){
        //             artist
        //         }
        //     };

        //     var track2 = new FullTrack() {
        //         Name = "Test",
        //         Album = new SimpleAlbum() {
        //             Name = "test",
        //             Artists = new List<SimpleArtist>(){
        //                 artist
        //             }
        //         },
        //         Artists = new List<SimpleArtist>(){
        //             artist
        //         }
        //     };

        //     track.Should().Be(track2);
        // }
    }
}
