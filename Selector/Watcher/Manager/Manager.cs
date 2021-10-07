using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class Manager: IManager, IDisposable
    {
        public bool IsRunning { get; private set; } = true;
        private List<WatcherContext> Watchers { get; set; } = new();
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
            var context = WatcherContext.From(watcher);
            if (IsRunning) context.Start();
            
            Watchers.Add(context);
        }

        public IEnumerable<WatcherContext> Running
            => Watchers.Where(w => w.IsRunning);

        public void Start()
        {
            foreach(var watcher in Watchers)
            {
                watcher.Start();
            }
            IsRunning = true;
        }
        
        public void Stop()
        {
            foreach(var watcher in Watchers)
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
    }
}
