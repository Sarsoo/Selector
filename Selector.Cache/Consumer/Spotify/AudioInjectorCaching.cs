using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    [Obsolete]
    public class CachingAudioFeatureInjector : AudioFeatureInjector
    {
        private readonly IDatabaseAsync Db;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromDays(14);

        public CachingAudioFeatureInjector(
            ISpotifyPlayerWatcher watcher,
            IDatabaseAsync db,
            ITracksClient trackClient,
            ILogger<CachingAudioFeatureInjector> logger = null,
            CancellationToken token = default
        ) : base(watcher, trackClient, logger, token)
        {
            Db = db;

            NewFeature += CacheCallback;
        }

        public void CacheCallback(object sender, AnalysedTrack e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await AsyncCacheCallback(e);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public async Task AsyncCacheCallback(AnalysedTrack e)
        {
            var payload = JsonSerializer.Serialize(e.Features, SpotifyJsonContext.Default.TrackAudioFeatures);

            Logger.LogTrace("Caching current for [{track}]", e.Track.DisplayString());

            var resp = await Db.StringSetAsync(Key.AudioFeature(e.Track.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug("Cached audio feature for [{track}], {state}", e.Track.DisplayString(),
                (resp ? "value set" : "value NOT set"));
        }
    }
}