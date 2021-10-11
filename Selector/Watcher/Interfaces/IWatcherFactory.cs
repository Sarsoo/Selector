using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IWatcherFactory
    {
        public IWatcher Create();
    }
}
