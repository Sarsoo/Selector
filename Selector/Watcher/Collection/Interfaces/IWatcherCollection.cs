using System;
using System.Collections.Generic;
using System.Text;

namespace Selector
{
    public interface IWatcherCollection
    {
        public bool IsRunning { get; }
        /// <summary>
        /// Add watcher to collection, will start watcher if collection is running
        /// </summary>
        /// <param name="watcher">New watcher</param>
        public void Add(IWatcher watcher);
        /// <summary>
        /// Add watcher with given consumers to collection, will start watcher if collection is running
        /// </summary>
        /// <param name="watcher">New watcher</param>
        /// <param name="consumers">Consumers to subscribe to new watcher</param>
        public void Add(IWatcher watcher, List<IConsumer> consumers);

        /// <summary>
        /// Start watcher collection
        /// </summary>
        public void Start();
        /// <summary>
        /// Stop watcher collection
        /// </summary>
        public void Stop();
    }
}
