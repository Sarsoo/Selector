using Selector.Spotify.ConfigFactory;

namespace Selector.Spotify
{
    public interface ISpotifyWatcherFactory
    {
        public Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, string id, int pollPeriod)
            where T : class, IWatcher;
    }
}