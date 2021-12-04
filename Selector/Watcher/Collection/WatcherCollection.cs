using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class WatcherCollection: IWatcherCollection, IDisposable, IEnumerable<WatcherContext>
    {
        private readonly ILogger<WatcherCollection> Logger;
        public bool IsRunning { get; private set; } = false;
        private List<WatcherContext> Watchers { get; set; } = new();

        public WatcherCollection(ILogger<WatcherCollection> logger = null)
        {
            Logger = logger ?? NullLogger<WatcherCollection>.Instance;
        }

        public int Count => Watchers.Count;
        public IEnumerable<Task> Tasks 
            => Watchers
                .Select(w => w.Task)
                .Where(t => t is not null);

        public IEnumerable<CancellationTokenSource> TokenSources 
            => Watchers
                .Select(w => w.TokenSource)
                .Where(t => t is not null);

        public void Add(IWatcher watcher)
        {
            Add(watcher, default);
        }

        public void Add(IWatcher watcher, List<IConsumer> consumers)
        {
            var context = WatcherContext.From(watcher, consumers);
            if (IsRunning) context.Start();

            Watchers.Add(context);
        }

        public IEnumerable<WatcherContext> Running
            => Watchers.Where(w => w.IsRunning);

        public void Start()
        {
            Logger.LogDebug($"Starting {Count} watcher(s)");
            foreach(var watcher in Watchers)
            {
                watcher.Start();
            }
            IsRunning = true;
        }
        
        public void Stop()
        {
            Logger.LogDebug($"Stopping {Count} watcher(s)");
            foreach (var watcher in Watchers)
            {
                watcher.Stop();
            }
            Task.WaitAll(Tasks.ToArray());
            IsRunning = false;
        }

        public void Dispose()
        {
            foreach(var watcher in Watchers)
            {
                watcher.Dispose();
            }
        }

        public IEnumerator<WatcherContext> GetEnumerator() => Watchers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
