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

        public async Task<TrackAudioFeatures> Get(string refreshToken, string trackId)
        {
            if(string.IsNullOrWhiteSpace(trackId)) throw new ArgumentNullException("No track Id provided");

            var track = await Cache.StringGetAsync(Key.AudioFeature(trackId));
            if (track == RedisValue.Null)
            {
                if(!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var factory = await SpotifyFactory.GetFactory(refreshToken);
                    var spotifyClient = new SpotifyClient(await factory.GetConfig());

                    // TODO: Error checking
                    return await spotifyClient.Tracks.GetAudioFeatures(trackId);
                }
                else 
                {
                    return null;
                }
            }
            else
            {
                var deserialised = JsonSerializer.Deserialize<TrackAudioFeatures>(track);
                return deserialised;
            }
        }

    }
}