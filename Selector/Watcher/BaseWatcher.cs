using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public abstract class BaseWatcher: IWatcher
    {
        public abstract Task WatchOne(CancellationToken token);

        public async Task Watch(CancellationToken cancelToken)
        {
            while (true) {
                cancelToken.ThrowIfCancellationRequested();
                await WatchOne(cancelToken);
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
