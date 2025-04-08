using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using Selector.Spotify.Timeline;
using SpotifyAPI.Web;
using Xunit;

namespace Selector.Tests
{
    [Obsolete]
    public class AudioInjectorTests
    {
        [Fact]
        public void Subscribe()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                NullLogger<AudioFeatureInjector>.Instance);

            featureInjector.Subscribe();

            watcherMock.VerifyAdd(m => m.ItemChange += It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());
        }

        [Fact]
        public void Unsubscribe()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                NullLogger<AudioFeatureInjector>.Instance);

            featureInjector.Unsubscribe();

            watcherMock.VerifyRemove(m => m.ItemChange -= It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());
        }

        [Fact]
        public void SubscribeFuncArg()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var watcherFuncArgMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                NullLogger<AudioFeatureInjector>.Instance);

            featureInjector.Subscribe(watcherFuncArgMock.Object);

            watcherFuncArgMock.VerifyAdd(m =>
                m.ItemChange += It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());
            watcherMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void UnsubscribeFuncArg()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var watcherFuncArgMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                NullLogger<AudioFeatureInjector>.Instance);

            featureInjector.Unsubscribe(watcherFuncArgMock.Object);

            watcherFuncArgMock.VerifyRemove(m =>
                m.ItemChange -= It.IsAny<EventHandler<SpotifyListeningChangeEventArgs>>());
            watcherMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CallbackNoId()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();
            var timelineMock = new Mock<AnalysedTrackTimeline>();
            var eventArgsMock = new Mock<SpotifyListeningChangeEventArgs>();
            var playingMock = new Mock<CurrentlyPlayingContext>();
            var trackMock = new Mock<FullTrack>();
            var featureMock = new Mock<TrackAudioFeatures>();

            eventArgsMock.Object.Current = playingMock.Object;
            playingMock.Object.Item = trackMock.Object;

            spotifyMock.Setup(m => m.GetAudioFeatures(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result)
                .Returns(() => featureMock.Object);

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                logger: NullLogger<AudioFeatureInjector>.Instance)
            {
                Timeline = timelineMock.Object
            };

            await (Task)typeof(AudioFeatureInjector)
                .GetMethod("ProcessEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(featureInjector, new object[] { eventArgsMock.Object });

            spotifyMock.VerifyNoOtherCalls();
            timelineMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CallbackWithId()
        {
            var watcherMock = new Mock<ISpotifyPlayerWatcher>();
            var spotifyMock = new Mock<ITracksClient>();
            var timelineMock = new Mock<AnalysedTrackTimeline>();
            var eventArgsMock = new Mock<SpotifyListeningChangeEventArgs>();
            var playingMock = new Mock<CurrentlyPlayingContext>();
            var trackMock = new Mock<FullTrack>();
            var featureMock = new Mock<TrackAudioFeatures>();

            eventArgsMock.Object.Current = playingMock.Object;
            playingMock.Object.Item = trackMock.Object;
            trackMock.Object.Id = "Fake-Id";

            spotifyMock.Setup(m => m.GetAudioFeatures(It.IsAny<string>(), It.IsAny<CancellationToken>()).Result)
                .Returns(() => featureMock.Object);

            var featureInjector = new AudioFeatureInjector(watcherMock.Object, spotifyMock.Object,
                logger: NullLogger<AudioFeatureInjector>.Instance)
            {
                Timeline = timelineMock.Object
            };

            await (Task)typeof(AudioFeatureInjector)
                .GetMethod("ProcessEvent", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(featureInjector, new object[] { eventArgsMock.Object });

            spotifyMock.Verify(m => m.GetAudioFeatures(It.IsAny<string>(), It.IsAny<CancellationToken>()));
            spotifyMock.VerifyNoOtherCalls();

            timelineMock.Verify(m => m.Add(It.IsAny<AnalysedTrack>(), It.IsAny<DateTime>()), Times.Once);
            timelineMock.VerifyNoOtherCalls();
        }
    }
}