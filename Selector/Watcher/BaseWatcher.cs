using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector
{
    public abstract class BaseWatcher: IWatcher
    {
        protected readonly ILogger<BaseWatcher> Logger;
        public string Id { get; set; }
        public string SpotifyUsername { get; set; }
        private Stopwatch ExecutionTimer { get; set; }

        public BaseWatcher(ILogger<BaseWatcher> logger = null)
        {
            Logger = logger ?? NullLogger<BaseWatcher>.Instance;
            ExecutionTimer = new Stopwatch();
        }

        public abstract Task WatchOne(CancellationToken token);
        public abstract Task Reset();

        public async Task Watch(CancellationToken cancelToken)
        {
            Logger.LogDebug("Starting watcher");
            while (true) {

                ExecutionTimer.Start();

                cancelToken.ThrowIfCancellationRequested();

                try
                {
                    await WatchOne(cancelToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception occured while conducting single poll operation");
                }

                ExecutionTimer.Stop();
                var waitTime = decimal.ToInt32(Math.Max(0, PollPeriod - ExecutionTimer.ElapsedMilliseconds));
                ExecutionTimer.Reset();

                Logger.LogTrace($"Finished watch one, delaying \"{PollPeriod}\"ms (\"{waitTime}\"ms)...");
                await Task.Delay(waitTime, cancelToken);
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
