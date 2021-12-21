using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class CachingAudioFeatureInjector : AudioFeatureInjector
    {
        private readonly IDatabaseAsync Db;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromDays(14);

        public CachingAudioFeatureInjector(
            IPlayerWatcher watcher,
            IDatabaseAsync db,
            ITracksClient trackClient,
            ILogger<CachingAudioFeatureInjector> logger = null,
            CancellationToken token = default
        ) : base(watcher, trackClient, logger, token) {

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
            var payload = JsonSerializer.Serialize(e.Features, JsonContext.Default.TrackAudioFeatures);
            
            Logger.LogTrace($"Caching current for [{e.Track.DisplayString()}]");

            var resp = await Db.StringSetAsync(Key.AudioFeature(e.Track.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug($"Cached audio feature for [{e.Track.DisplayString()}], {(resp ? "value set" : "value NOT set")}");
        }
    }
}
