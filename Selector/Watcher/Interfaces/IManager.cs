using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    interface IManager
    {
        public void Add(IWatcher watcher);
        
        public bool Start();
        public bool Stop();
    }
}
