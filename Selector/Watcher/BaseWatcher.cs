using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public abstract class BaseWatcher: IWatcher
    {
        public abstract Task WatchOne();

        public async Task Watch(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await WatchOne();
                await Task.Delay(PollPeriod);
            }
        }

        private int _pollPeriod;
        public int PollPeriod
        {
            get => _pollPeriod;
            set => _pollPeriod = Math.Max(0, value);
        }
    }
}
