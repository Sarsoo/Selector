using System;
using System.Text.Json;
using System.Threading.Tasks;
using Selector.Spotify;
using Selector.Spotify.FactoryProvider;
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
            IDatabaseAsync cache = null
        )
        {
            SpotifyFactory = spotifyFactory;
            Cache = cache;
        }

        public async Task<TrackAudioFeatures> Get(string refreshToken, string trackId)
        {
            if (string.IsNullOrWhiteSpace(trackId)) throw new ArgumentNullException("No track Id provided");

            var track = await Cache?.StringGetAsync(Key.AudioFeature(trackId));
            if (Cache is null || track == RedisValue.Null)
            {
                if (!string.IsNullOrWhiteSpace(refreshToken) && !Magic.Dummy)
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
                var deserialised = JsonSerializer.Deserialize(track, SpotifyJsonContext.Default.TrackAudioFeatures);
                return deserialised;
            }
        }
    }
}