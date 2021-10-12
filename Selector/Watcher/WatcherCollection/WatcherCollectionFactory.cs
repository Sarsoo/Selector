using Microsoft.Extensions.Logging;

namespace Selector
{
    public class WatcherCollectionFactory: IWatcherCollectionFactory
    {
        private readonly ILoggerFactory LoggerFactory;

        public WatcherCollectionFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public IWatcherCollection Get()
        {
            return new WatcherCollection(LoggerFactory.CreateLogger<WatcherCollection>());
        }
    }
}
