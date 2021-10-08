using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IWatcherCollection
    {
        public bool IsRunning { get; }
        public void Add(IWatcher watcher);
        
        public void Start();
        public void Stop();
    }
}
