using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IWatcherContext
    {
        void AddConsumer(IConsumer consumer);
        void Start();
        void Stop();
    }
}
