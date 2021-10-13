using System;
using System.Threading.Tasks;

namespace Selector
{
    public interface IConsumer
    {
        public void Callback(object sender, ListeningChangeEventArgs e);
        public void Subscribe(IWatcher watch = null);
        public void Unsubscribe(IWatcher watch = null);
    }
}
