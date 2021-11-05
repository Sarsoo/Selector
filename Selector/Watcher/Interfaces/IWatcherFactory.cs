using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Selector
{
    public interface IWatcherFactory
    {
        public Task<IWatcher> Get<T>(ISpotifyConfigFactory spotifyFactory, string id, int pollPeriod)
            where T : class, IWatcher;
    }
}
