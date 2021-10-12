using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class WatcherContext: IDisposable
    {
        public IWatcher Watcher { get; set; }
        public bool IsRunning { get; private set; }
        public Task Task { get; set; }
        public CancellationTokenSource TokenSource { get; set; }

        public WatcherContext(IWatcher watcher)
        {
            Watcher = watcher;
        }

        public static WatcherContext From(IWatcher watcher)
        {
            return new(watcher);
        }

        public void Start()
        {
            if (IsRunning)
                Stop();
            
            IsRunning = true;
            TokenSource = new();
            Task = Watcher.Watch(TokenSource.Token);
            Task.ContinueWith(t =>
            {
                if (t.Exception != null) throw t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Stop()
        {
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
