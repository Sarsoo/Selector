using System;
using System.Text.Json;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class AudioFeaturePuller
    {
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;
        private readonly IDatabaseAsync Cache;

        public AudioFeaturePuller(
            IRefreshTokenFactoryProvider spotifyFactory,
            IDatabaseAsync cache
        )
        {
            SpotifyFactory = spotifyFactory;
            Cache = cache;
        }

        public async Task<TrackAudioFeatures> Get(string userId, string trackId)
        {
            var track = await Cache.StringGetAsync(Key.AudioFeature(trackId));
            if (track == RedisValue.Null)
            {
                // TODO: finish implementing network pull
                // return await SpotifyClient.GetAudioFeatures(trackId);
                throw new NotImplementedException("Can't pull over network yet");
            }
            else
            {
                var deserialised = JsonSerializer.Deserialize<TrackAudioFeatures>(track);
                return deserialised;
            }
        }

    }
}