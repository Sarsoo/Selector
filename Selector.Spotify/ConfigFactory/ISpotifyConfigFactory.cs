using SpotifyAPI.Web;

namespace Selector.Spotify.ConfigFactory
{
    public interface ISpotifyConfigFactory
    {
        public Task<SpotifyClientConfig> GetConfig();
    }
}