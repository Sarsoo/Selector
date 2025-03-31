using Microsoft.Extensions.Logging;
using Selector.Extensions;
using SpotifyAPI.Web;

namespace Selector.Spotify.Consumer
{
    public class DummyAudioFeatureInjector : AudioFeatureInjector
    {
        private TrackAudioFeatures[] _features = new[]
        {
            new TrackAudioFeatures
            {
                Acousticness = 0.5f,
                Danceability = 0.5f,
                DurationMs = 100,
                Energy = 0.5f,
                Instrumentalness = 0.5f,
                Key = 5,
                Liveness = 0.5f,
                Loudness = 0.5f,
                Mode = 1,
                Speechiness = 0.5f,
                Tempo = 120f,
                TimeSignature = 4,
                Valence = 0.5f,
            }
        };

        private int _contextIdx = 0;

        private DateTime _lastNext = DateTime.UtcNow;
        private TimeSpan _contextLifespan = TimeSpan.FromSeconds(30);

        public DummyAudioFeatureInjector(
            ISpotifyPlayerWatcher watcher,
            ILogger<DummyAudioFeatureInjector> logger = null,
            CancellationToken token = default
        ) : base(watcher, null, logger, token)
        {
        }

        private bool ShouldCycle() => DateTime.UtcNow - _lastNext > _contextLifespan;

        private void BumpContextIndex()
        {
            Logger.LogDebug("Next feature");

            _contextIdx++;

            if (_contextIdx >= _features.Length)
            {
                _contextIdx = 0;
            }
        }

        private TrackAudioFeatures GetFeature()
        {
            if (ShouldCycle()) BumpContextIndex();

            return _features[_contextIdx];
        }

        public override Task AsyncCallback(SpotifyListeningChangeEventArgs e)
        {
            using var scope = Logger.GetListeningEventArgsScope(e);

            if (e.Current.Item is FullTrack track)
            {
                if (string.IsNullOrWhiteSpace(track.Id)) return Task.CompletedTask;

                var audioFeatures = GetFeature();

                var analysedTrack = AnalysedTrack.From(track, audioFeatures);

                Timeline.Add(analysedTrack, DateHelper.FromUnixMilli(e.Current.Timestamp));
                OnNewFeature(analysedTrack);
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                if (string.IsNullOrWhiteSpace(episode.Id)) return Task.CompletedTask;

                Logger.LogDebug("Ignoring podcast episdoe [{episode}]", episode.DisplayString());
            }
            else if (e.Current.Item is null)
            {
                Logger.LogDebug("Skipping audio feature pulling for null item [{context}]", e.Current.DisplayString());
            }
            else
            {
                Logger.LogError("Unknown item pulled from API [{item}]", e.Current.Item);
            }

            return Task.CompletedTask;
        }
    }
}