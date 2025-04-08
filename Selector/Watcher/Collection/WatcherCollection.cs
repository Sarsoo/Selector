using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector
{
    public class WatcherCollection : IWatcherCollection, IDisposable, IEnumerable<WatcherContext>
    {
        private readonly ILogger<WatcherCollection> Logger;
        public bool IsRunning { get; private set; } = false;
        private List<WatcherContext> Watchers { get; set; } = new();

        public WatcherCollection(ILogger<WatcherCollection>? logger = null)
        {
            Logger = logger ?? NullLogger<WatcherCollection>.Instance;
        }

        public int Count => Watchers.Count;

        public IEnumerable<IConsumer> Consumers
            => Watchers
                .SelectMany(w => w.Consumers)
                .Where(t => t is not null);

        public IEnumerable<Task> Tasks
            => Watchers
                .Select(w => w.Task)
                .Where(t => t is not null)
                .Select(t => t!);

        public IEnumerable<CancellationTokenSource?> TokenSources
            => Watchers
                .Select(w => w.TokenSource)
                .Where(t => t is not null);

        public IWatcherContext Add(IWatcher watcher)
        {
            return Add(watcher, Enumerable.Empty<IConsumer>());
        }

        public IWatcherContext Add(IWatcher watcher, IEnumerable<IConsumer> consumers)
        {
            var context = WatcherContext.From(watcher, consumers);
            if (IsRunning) context.Start();

            Watchers.Add(context);
            return context;
        }

        public IEnumerable<WatcherContext> Running
            => Watchers.Where(w => w.IsRunning);

        public void Start()
        {
            if (IsRunning) return;

            Logger.LogDebug("Starting {} watcher(s)", Count);
            foreach (var watcher in Watchers)
            {
                watcher.Start();
            }

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            try
            {
                Logger.LogDebug("Stopping {} watcher(s)", Count);
                foreach (var watcher in Watchers)
                {
                    watcher.Stop();
                }

                Task.WaitAll(Tasks.ToArray());
                IsRunning = false;
            }
            catch (TaskCanceledException)
            {
                Logger.LogTrace("Caught task cancelled exception");
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TaskCanceledException ||
                    ex.InnerExceptions.Any(e => e is TaskCanceledException))
                {
                    Logger.LogTrace("Caught task cancelled exception");
                }
                else
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            foreach (var watcher in Watchers)
            {
                watcher.Dispose();
            }
        }

        public IEnumerator<WatcherContext> GetEnumerator() => Watchers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}