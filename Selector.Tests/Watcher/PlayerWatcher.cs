using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Selector.Spotify.Equality;
using Selector.Spotify.Watcher;
using SpotifyAPI.Web;
using Xunit;

namespace Selector.Tests
{
    public class SpotifyPlayerWatcherTests
    {
        public static IEnumerable<object[]> NowPlayingData =>
            new List<object[]>
            {
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("track1", "album1", "artist1")),
                        Helper.CurrentPlayback(Helper.FullTrack("track2", "album2", "artist2")),
                        Helper.CurrentPlayback(Helper.FullTrack("track3", "album3", "artist3")),
                    }
                }
            };

        [Theory]
        [MemberData(nameof(NowPlayingData))]
        public async Task NowPlaying(List<CurrentlyPlayingContext> playing)
        {
            var playingQueue = new Queue<CurrentlyPlayingContext>(playing);

            var spotMock = new Mock<IPlayerClient>();
            var eq = new UriEqual();

            spotMock.Setup(s => s.GetCurrentPlayback(It.IsAny<CancellationToken>()).Result)
                .Returns(playingQueue.Dequeue);

            var watcher = new SpotifyPlayerWatcher(spotMock.Object, eq);

            for (var i = 0; i < playing.Count; i++)
            {
                await watcher.WatchOne();
                watcher.Live.Should().Be(playing[i]);
            }
        }

        public static IEnumerable<object[]> EventsData =>
            new List<object[]>
            {
                // NO CHANGING
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("nochange", "album1", "artist1"), isPlaying: true,
                            context: "context1"),
                        Helper.CurrentPlayback(Helper.FullTrack("nochange", "album1", "artist1"), isPlaying: true,
                            context: "context1"),
                        Helper.CurrentPlayback(Helper.FullTrack("nochange", "album1", "artist1"), isPlaying: true,
                            context: "context1"),
                    },
                    // to raise
                    new List<string>()
                        { "ItemChange", "ContextChange", "PlayingChange", "DeviceChange", "VolumeChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // TRACK CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("trackchange1", "album1", "artist1")),
                        Helper.CurrentPlayback(Helper.FullTrack("trackchange2", "album1", "artist1"))
                    },
                    // to raise
                    new List<string>()
                        { "ContextChange", "PlayingChange", "ItemChange", "DeviceChange", "VolumeChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // ALBUM CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("albumchange", "album1", "artist1")),
                        Helper.CurrentPlayback(Helper.FullTrack("albumchange", "album2", "artist1"))
                    },
                    // to raise
                    new List<string>()
                    {
                        "ContextChange", "PlayingChange", "ItemChange", "AlbumChange", "DeviceChange", "VolumeChange"
                    },
                    // to not raise
                    new List<string>() { "ArtistChange" }
                },
                // ARTIST CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("artistchange", "album1", "artist1")),
                        Helper.CurrentPlayback(Helper.FullTrack("artistchange", "album1", "artist2"))
                    },
                    // to raise
                    new List<string>()
                    {
                        "ContextChange", "PlayingChange", "ItemChange", "ArtistChange", "DeviceChange", "VolumeChange"
                    },
                    // to not raise
                    new List<string>() { "AlbumChange" }
                },
                // CONTEXT CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("contextchange", "album1", "artist1"),
                            context: "context1"),
                        Helper.CurrentPlayback(Helper.FullTrack("contextchange", "album1", "artist1"),
                            context: "context2")
                    },
                    // to raise
                    new List<string>()
                        { "PlayingChange", "ItemChange", "ContextChange", "DeviceChange", "VolumeChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // PLAYING CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("playingchange1", "album1", "artist1"), isPlaying: true,
                            context: "context1"),
                        Helper.CurrentPlayback(Helper.FullTrack("playingchange1", "album1", "artist1"),
                            isPlaying: false, context: "context1")
                    },
                    // to raise
                    new List<string>()
                        { "ContextChange", "ItemChange", "PlayingChange", "DeviceChange", "VolumeChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // PLAYING CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("playingchange2", "album1", "artist1"),
                            isPlaying: false, context: "context1"),
                        Helper.CurrentPlayback(Helper.FullTrack("playingchange2", "album1", "artist1"), isPlaying: true,
                            context: "context1")
                    },
                    // to raise
                    new List<string>()
                        { "ContextChange", "ItemChange", "PlayingChange", "DeviceChange", "VolumeChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // CONTENT CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("contentchange1", "album1", "artist1"), isPlaying: true,
                            context: "context1"),
                        Helper.CurrentPlayback(Helper.FullEpisode("contentchange1", "show1", "pub1"), isPlaying: true,
                            context: "context2")
                    },
                    // to raise
                    new List<string>()
                    {
                        "PlayingChange", "ContentChange", "ContextChange", "ItemChange", "DeviceChange", "VolumeChange"
                    },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // CONTENT CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullEpisode("contentchange1", "show1", "pub1"), isPlaying: true,
                            context: "context2"),
                        Helper.CurrentPlayback(Helper.FullTrack("contentchange1", "album1", "artist1"), isPlaying: true,
                            context: "context1")
                    },
                    // to raise
                    new List<string>()
                    {
                        "PlayingChange", "ContentChange", "ContextChange", "ItemChange", "DeviceChange", "VolumeChange"
                    },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange" }
                },
                // DEVICE CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("devicechange", "album1", "artist1"),
                            device: Helper.Device("dev1")),
                        Helper.CurrentPlayback(Helper.FullTrack("devicechange", "album1", "artist1"),
                            device: Helper.Device("dev2"))
                    },
                    // to raise
                    new List<string>()
                        { "ContextChange", "PlayingChange", "ItemChange", "VolumeChange", "DeviceChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange", "ContentChange" }
                },
                // VOLUME CHANGE
                new object[]
                {
                    new List<CurrentlyPlayingContext>()
                    {
                        Helper.CurrentPlayback(Helper.FullTrack("volumechange", "album1", "artist1"),
                            device: Helper.Device("dev1", volume: 50)),
                        Helper.CurrentPlayback(Helper.FullTrack("volumechange", "album1", "artist1"),
                            device: Helper.Device("dev1", volume: 60))
                    },
                    // to raise
                    new List<string>()
                        { "ContextChange", "PlayingChange", "ItemChange", "VolumeChange", "DeviceChange" },
                    // to not raise
                    new List<string>() { "AlbumChange", "ArtistChange", "ContentChange" }
                },
                // // STARTED PLAYBACK
                // new object[] { new List<CurrentlyPlayingContext>(){
                //         null,
                //         Helper.CurrentPlayback(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1")
                //     },
                //     // to raise
                //     new List<string>(){ "PlayingChange" },
                //     // to not raise
                //     new List<string>(){ "AlbumChange", "ArtistChange", "ContentChange", "ContextChange", "ItemChange", "DeviceChange", "VolumeChange" }
                // },
                // // STARTED PLAYBACK
                // new object[] { new List<CurrentlyPlayingContext>(){
                //         Helper.CurrentPlayback(Helper.FullTrack("track1", "album1", "artist1"), isPlaying: true, context: "context1"),
                //         null
                //     },
                //     // to raise
                //     new List<string>(){ "PlayingChange" },
                //     // to not raise
                //     new List<string>(){ "AlbumChange", "ArtistChange", "ContentChange", "ContextChange", "ItemChange", "DeviceChange", "VolumeChange" }
                // }
            };

        [Theory]
        [MemberData(nameof(EventsData))]
        public async Task Events(List<CurrentlyPlayingContext> playing, List<string> toRaise, List<string> toNotRaise)
        {
            var playingQueue = new Queue<CurrentlyPlayingContext>(playing);

            var spotMock = new Mock<IPlayerClient>();
            var eq = new UriEqual();

            spotMock.Setup(
                s => s.GetCurrentPlayback(It.IsAny<CancellationToken>()).Result
            ).Returns(playingQueue.Dequeue);

            var watcher = new SpotifyPlayerWatcher(spotMock.Object, eq);
            using var monitoredWatcher = watcher.Monitor();

            for (var i = 0; i < playing.Count; i++)
            {
                await watcher.WatchOne();
            }

            toRaise.ForEach(r => monitoredWatcher.Should().Raise(r).WithSender(watcher));
            toNotRaise.ForEach(r => monitoredWatcher.Should().NotRaise(r));
        }

        [Theory]
        [InlineData(1000, 3500, 4)]
        [InlineData(500, 3800, 8)]
        [InlineData(100, 250, 3)]
        public async Task Watch(int pollPeriod, int execTime, int numberOfCalls)
        {
            var spotMock = new Mock<IPlayerClient>();
            var eq = new UriEqual();
            var watch = new SpotifyPlayerWatcher(spotMock.Object, eq)
            {
                PollPeriod = pollPeriod
            };

            var tokenSource = new CancellationTokenSource();
            var task = watch.Watch(tokenSource.Token);

            await Task.Delay(execTime);
            tokenSource.Cancel();

            spotMock.Verify(s => s.GetCurrentPlayback(It.IsAny<CancellationToken>()), Times.Exactly(numberOfCalls));
        }

        // [Fact]
        // public async void Auth()
        // {
        //     var spot = new SpotifyClient("");
        //     var eq = new UriEqual();
        //     var watch = new PlayerWatcher(spot.Player, eq);

        //     var token = new CancellationTokenSource();
        //     await watch.Watch(token.Token);
        // }
    }
}