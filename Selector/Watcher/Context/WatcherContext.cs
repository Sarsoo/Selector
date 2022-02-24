using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class WatcherContext: IDisposable, IWatcherContext
    {
        public IWatcher Watcher { get; set; }
        public List<IConsumer> Consumers { get; private set; } = new();
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Reference to Watcher.Watch() task when running
        /// </summary>
        /// <value></value>
        public Task Task { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public WatcherContext(IWatcher watcher)
        {
            Watcher = watcher;
        }

        public WatcherContext(IWatcher watcher, IEnumerable<IConsumer> consumers)
        {
            Watcher = watcher;
            Consumers = consumers.ToList() ?? new();
        }

        public static WatcherContext From(IWatcher watcher)
        {
            return new(watcher);
        }

        public static WatcherContext From(IWatcher watcher, IEnumerable<IConsumer> consumers)
        {
            return new(watcher, consumers);
        }

        public void AddConsumer(IConsumer consumer)
        {
            if (IsRunning)
                consumer.Subscribe(Watcher);

            Consumers.Add(consumer);
        }

        public void Start()
        {
            if (IsRunning)
                Stop();
            
            IsRunning = true;
            TokenSource = new();

            Consumers.ForEach(c => c.Subscribe(Watcher));

            Reset();
        }

        private void Reset()
        {
            if(Task is not null && !Task.IsCompleted)
            {
                TokenSource.Cancel();
            }

            TokenSource = new();

            Task = Watcher.Watch(TokenSource.Token);
            Task.ContinueWith(t =>
            {
                if (t.Exception != null) throw t.Exception;
                Watcher.Reset();
                Reset();
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Stop()
        {
            Consumers.ForEach(c => c.Unsubscribe(Watcher));

            TokenSource.Cancel();
            IsRunning = false;
        }

        private void Clear()
        {
            if(IsRunning 
            || Task.Status == TaskStatus.Running
            || Task.Status == TaskStatus.WaitingToRun)
            {
                Stop();
            }
            
            Task = null;
            TokenSource = null;
        }

        public void Dispose()
        {
            Stop();
            Clear();
        }
    }
}
