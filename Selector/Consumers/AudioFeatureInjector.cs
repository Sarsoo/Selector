using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SpotifyAPI.Web;

namespace Selector
{
    public class AudioFeatureInjector : IConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly ITracksClient TrackClient;
        protected readonly ILogger<AudioFeatureInjector> Logger;

        protected event EventHandler<AnalysedTrack> NewFeature;

        public CancellationToken CancelToken { get; set; }

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        public AudioFeatureInjector(
            IPlayerWatcher watcher, 
            ITracksClient trackClient, 
            ILogger<AudioFeatureInjector> logger = null,
            CancellationToken token = default
        ){
            Watcher = watcher;
            TrackClient = trackClient;
            Logger = logger ?? NullLogger<AudioFeatureInjector>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            Task.Run(async () => { await AsyncCallback(e); }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            if (e.Current.Item is FullTrack track)
            {
                try {
                    Logger.LogTrace("Making Spotify call");
                    var audioFeatures = await TrackClient.GetAudioFeatures(track.Id);
                    Logger.LogDebug($"Adding audio features [{track.DisplayString()}]: [{audioFeatures.DisplayString()}]");

                    var analysedTrack = AnalysedTrack.From(track, audioFeatures);

                    Timeline.Add(analysedTrack, DateHelper.FromUnixMilli(e.Current.Timestamp));
                    OnNewFeature(analysedTrack);
                }
                catch (APIUnauthorizedException ex)
                {
                    Logger.LogDebug($"Unauthorised error: [{ex.Message}] (should be refreshed and retried?)");
                    //throw ex;
                }
                catch (APITooManyRequestsException ex)
                {
                    Logger.LogDebug($"Too many requests error: [{ex.Message}]");
                    throw ex;
                }
                catch (APIException ex)
                {
                    Logger.LogDebug($"API error: [{ex.Message}]");
                    throw ex;
                }
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                Logger.LogDebug($"Ignoring podcast episdoe [{episode.DisplayString()}]");
            }
            else
            {
                Logger.LogError($"Unknown item pulled from API [{e.Current.Item}]");
            }
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange += Callback;
            } 
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        protected virtual void OnNewFeature(AnalysedTrack args)
        {
            NewFeature?.Invoke(this, args); 
        }
    }

    public class AnalysedTrack {
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
