using System.Threading.Tasks;

namespace Selector
{
    public interface ISpotifyWatcherFactory
    {
        public Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, string id, int pollPeriod)
            where T : class, IWatcher;
    }
}