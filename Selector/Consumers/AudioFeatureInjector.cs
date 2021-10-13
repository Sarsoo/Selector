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
        private readonly IPlayerWatcher Watcher;
        private readonly ITracksClient TrackClient;
        private readonly ILogger<AudioFeatureInjector> Logger;

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

            Task.Run(() => { return AsyncCallback(e); }, CancelToken);
        }

        private async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            if (e.Current.Item is FullTrack track)
            {
                Logger.LogTrace("Making Spotify call");
                var audioFeatures = await TrackClient.GetAudioFeatures(track.Id);
                Logger.LogDebug($"Adding audio features [{track.DisplayString()}]: [{audioFeatures.DisplayString()}]");

                Timeline.Add(AnalysedTrack.From(track, audioFeatures));
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
