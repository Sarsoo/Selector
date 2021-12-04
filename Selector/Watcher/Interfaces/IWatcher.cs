using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public interface IWatcher
    {
        /// <summary>
        /// Make single poll action on remote resource
        /// </summary>
        /// <param name="cancelToken">Token for early cancell</param>
        /// <returns>awaitable task</returns>
        public Task WatchOne(CancellationToken cancelToken);
        /// <summary>
        /// Begin periodically polling with interval of PollPeriod
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task Watch(CancellationToken cancelToken);

        public Task Reset();

        /// <summary>
        /// Time interval in ms between polls from Watch()
        /// </summary>
        /// <value></value>
        public int PollPeriod { get; set; }
    }
}
