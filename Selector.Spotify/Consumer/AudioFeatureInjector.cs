using Microsoft.Extensions.Logging;
using Selector.Extensions;
using Selector.Spotify.Timeline;
using SpotifyAPI.Web;

namespace Selector.Spotify.Consumer
{
    [Obsolete]
    public class AudioFeatureInjector :
        BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>, ISpotifyPlayerConsumer
    {
        protected readonly ITracksClient TrackClient;

        protected event EventHandler<AnalysedTrack> NewFeature;

        public CancellationToken CancelToken { get; set; }

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        public AudioFeatureInjector(
            ISpotifyPlayerWatcher? watcher,
            ITracksClient trackClient,
            ILogger<AudioFeatureInjector> logger,
            CancellationToken token = default
        ) : base(watcher, logger)
        {
            TrackClient = trackClient;
            CancelToken = token;
        }

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            using var scope = Logger.GetListeningEventArgsScope(e);

            if (e.Current.Item is FullTrack track)
            {
                if (string.IsNullOrWhiteSpace(track.Id)) return;

                try
                {
                    Logger.LogTrace("Making Spotify call");
                    var audioFeatures = await TrackClient.GetAudioFeatures(track.Id);
                    Logger.LogDebug("Adding audio features [{track}]: [{audio_features}]", track.DisplayString(),
                        audioFeatures.DisplayString());

                    var analysedTrack = AnalysedTrack.From(track, audioFeatures);

                    Timeline.Add(analysedTrack, DateHelper.FromUnixMilli(e.Current.Timestamp));
                    OnNewFeature(analysedTrack);
                }
                catch (APIUnauthorizedException ex)
                {
                    Logger.LogDebug("Unauthorised error: [{message}] (should be refreshed and retried?)", ex.Message);
                    //throw ex;
                }
                catch (APITooManyRequestsException ex)
                {
                    Logger.LogDebug("Too many requests error: [{message}]", ex.Message);
                    throw;
                }
                catch (APIException ex)
                {
                    Logger.LogDebug("API error: [{message}]", ex.Message);
                    throw;
                }
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                if (string.IsNullOrWhiteSpace(episode.Id)) return;

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
        }

        protected virtual void OnNewFeature(AnalysedTrack args)
        {
            NewFeature?.Invoke(this, args);
        }
    }

    public class AnalysedTrack
    {
        public FullTrack Track { get; set; }
        public TrackAudioFeatures Features { get; set; }

        public static AnalysedTrack From(FullTrack track, TrackAudioFeatures features)
        {
            return new AnalysedTrack()
            {
                Track = track,
                Features = features
            };
        }
    }
}