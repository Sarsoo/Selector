using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Selector.Spotify.Watcher;
using SpotifyAPI.Web;
using Xunit;

namespace Selector.Tests
{
    public class PlaylistWatcherTests
    {
        public static IEnumerable<object[]> CurrentPlaylistData =>
            new List<object[]>
            {
                new object[]
                {
                    new List<FullPlaylist>()
                    {
                        Helper.FullPlaylist("playlist1"),
                        Helper.FullPlaylist("playlist1"),
                        Helper.FullPlaylist("playlist1"),
                        Helper.FullPlaylist("playlist1")
                    }
                }
            };

        [Theory]
        [MemberData(nameof(CurrentPlaylistData))]
        public async Task CurrentPlaylist(List<FullPlaylist> playing)
        {
            var playlistDequeue = new Queue<FullPlaylist>(playing);

            var spotMock = new Mock<ISpotifyClient>();

            spotMock.Setup(s => s.Playlists.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result)
                .Returns(playlistDequeue.Dequeue);

            var config = new PlaylistWatcherConfig() { PlaylistId = "spotify:playlist:test" };
            var watcher = new PlaylistWatcher(config, spotMock.Object) { Id = "test", SpotifyUsername = "test" };

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
                    new List<FullPlaylist>()
                    {
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot1"),
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot1"),
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot1"),
                    },
                    // to raise
                    new List<string>() { },
                    // to not raise
                    new List<string>() { "SnapshotChange" }
                },
                // CHANGING SNAPSHOT
                new object[]
                {
                    new List<FullPlaylist>()
                    {
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot1"),
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot2"),
                        Helper.FullPlaylist("Playlist", snapshotId: "snapshot2"),
                    },
                    // to raise
                    new List<string>() { "SnapshotChange" },
                    // to not raise
                    new List<string>() { }
                }
            };

        [Theory]
        [MemberData(nameof(EventsData))]
        public async Task Events(List<FullPlaylist> playing, List<string> toRaise, List<string> toNotRaise)
        {
            var playlistDequeue = new Queue<FullPlaylist>(playing);

            var spotMock = new Mock<ISpotifyClient>();

            spotMock.Setup(s => s.Playlists.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result)
                .Returns(playlistDequeue.Dequeue);

            var config = new PlaylistWatcherConfig() { PlaylistId = "spotify:playlist:test" };
            var watcher = new PlaylistWatcher(config, spotMock.Object) { Id = "test", SpotifyUsername = "test" };

            using var monitoredWatcher = watcher.Monitor();

            for (var i = 0; i < playing.Count; i++)
            {
                await watcher.WatchOne();
            }

            toRaise.ForEach(r => monitoredWatcher.Should().Raise(r).WithSender(watcher));
            toNotRaise.ForEach(r => monitoredWatcher.Should().NotRaise(r));
        }
    }
}