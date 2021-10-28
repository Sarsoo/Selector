using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector
{
    public abstract class BaseWatcher: IWatcher
    {
        protected readonly ILogger<BaseWatcher> Logger;
        public string Username { get; set; }

        public BaseWatcher(ILogger<BaseWatcher> logger = null)
        {
            Logger = logger ?? NullLogger<BaseWatcher>.Instance;
        }

        public abstract Task WatchOne(CancellationToken token);

        public async Task Watch(CancellationToken cancelToken)
        {
            Logger.LogDebug("Starting watcher");
            while (true) {
                cancelToken.ThrowIfCancellationRequested();
                await WatchOne(cancelToken);
                Logger.LogTrace($"Finished watch one, delaying {PollPeriod}ms...");
                await Task.Delay(PollPeriod, cancelToken);
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
