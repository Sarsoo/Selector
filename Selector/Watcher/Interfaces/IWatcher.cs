using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public interface IWatcher
    {
        public Task WatchOne(CancellationToken cancelToken);
        public Task Watch(CancellationToken cancelToken);

        public int PollPeriod { get; set; }
    }
}
