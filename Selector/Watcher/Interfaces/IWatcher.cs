using System;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Selector
{
    public interface IWatcher
    {
        public Task WatchOne();
        public Task Watch(CancellationToken cancelToken);

        public int PollPeriod { get; set; }
    }
}
