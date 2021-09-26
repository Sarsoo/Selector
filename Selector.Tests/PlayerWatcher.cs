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
        public static IEnumerable<object[]> NowPlayingData => 
        new List<object[]>
        {
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1")),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track2", "album2", "artist2")),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track3", "album3", "artist3")),
                } 
            }
        };

        [Theory]
        [MemberData(nameof(NowPlayingData))]
        public async void NowPlaying(List<CurrentlyPlaying> playing)
        {
            var playingQueue = new Queue<CurrentlyPlaying>(playing);

            var spotMock = new Mock<IPlayerClient>();
            var scheduleMock = new Mock<IScheduler>();
            var eq = new UriEquality();

            spotMock.Setup(s => s.GetCurrentlyPlaying(It.IsAny<PlayerCurrentlyPlayingRequest>()).Result).Returns(playingQueue.Dequeue);

            var watcher = new PlayerWatcher(spotMock.Object, scheduleMock.Object, eq);

            for(var i = 0; i < playing.Count; i++)
            {
                await watcher.WatchOne();
                var current = watcher.NowPlaying();
                current.Should().Be(playing[i]);
            }
        }

        public static IEnumerable<object[]> EventsData => 
        new List<object[]>
        {
            // NO CHANGING
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1"),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1"),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1"),
                },
                // to raise
                new List<string>(){ },
                // to not raise
                new List<string>(){ "TrackChange", "AlbumChange", "ArtistChange", "ContextChange", "PlayingChange" }
            },
            // TRACK CHANGE
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1")),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track2", "album1", "artist1"))
                },
                // to raise
                new List<string>(){ "TrackChange" },
                // to not raise
                new List<string>(){ "AlbumChange", "ArtistChange" }
            },
            // ALBUM CHANGE
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1")),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album2", "artist1"))
                },
                // to raise
                new List<string>(){ "TrackChange", "AlbumChange" },
                // to not raise
                new List<string>(){ "ArtistChange" }
            },
            // ARTIST CHANGE
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1")),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist2"))
                },
                // to raise
                new List<string>(){ "TrackChange", "AlbumChange", "ArtistChange" },
                // to not raise
                new List<string>(){ }
            },
            // CONTEXT CHANGE
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), context: "context1"),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), context: "context2")
                },
                // to raise
                new List<string>(){ "ContextChange" },
                // to not raise
                new List<string>(){ "TrackChange", "AlbumChange", "ArtistChange" }
            },
            // PLAYING CHANGE
            new object[] { new List<CurrentlyPlaying>(){
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1"),
                    Helper.CurrentlyPlaying(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: false, context: "context1")
                },
                // to raise
                new List<string>(){ "PlayingChange" },
                // to not raise
                new List<string>(){ "TrackChange", "AlbumChange", "ArtistChange", "ContextChange" }
            }
        };

        [Theory]
        [MemberData(nameof(EventsData))]
        public async void Events(List<CurrentlyPlaying> playing, List<string> toRaise, List<string> toNotRaise)
        {
            var playingQueue = new Queue<CurrentlyPlaying>(playing);

            var spotMock = new Mock<IPlayerClient>();
            var scheduleMock = new Mock<IScheduler>();
            var eq = new UriEquality();

            spotMock.Setup(s => s.GetCurrentlyPlaying(It.IsAny<PlayerCurrentlyPlayingRequest>()).Result).Returns(playingQueue.Dequeue);

            var watcher = new PlayerWatcher(spotMock.Object, scheduleMock.Object, eq);
            using var monitoredWatcher = watcher.Monitor();

            for(var i = 0; i < playing.Count; i++)
            {
                await watcher.WatchOne();
            }

            foreach(var raise in toRaise){
                monitoredWatcher.Should().Raise(raise).WithSender(watcher);
            }
            foreach(var notRraise in toNotRaise){
                monitoredWatcher.Should().NotRaise(notRraise);
            }
        }
    }
}
