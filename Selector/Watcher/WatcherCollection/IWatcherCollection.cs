using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IWatcherCollection
    {
        public bool IsRunning { get; }
        public void Add(IWatcher watcher);
        public void Add(IWatcher watcher, List<IConsumer> consumers);

        public void Start();
        public void Stop();
    }
}
