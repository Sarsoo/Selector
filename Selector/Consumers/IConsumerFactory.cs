using System;
using System.Threading.Tasks;

namespace Selector
{
    public interface IConsumerFactory
    {
        public Task<IConsumer> Get(ISpotifyConfigFactory spotifyFactory, IPlayerWatcher watcher);
    }
}
