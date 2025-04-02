using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public interface IConsumer
    {
        public void Subscribe(IWatcher watch = null);
        public void Unsubscribe(IWatcher watch = null);
    }

    public interface IProcessingConsumer
    {
        public Task ProcessQueue(CancellationToken token);
    }

    public interface IConsumer<T> : IConsumer
    {
        public void Callback(object sender, T e);
    }

    public interface IProcessingConsumer<T> : IConsumer<T>, IProcessingConsumer
    {
    }
}