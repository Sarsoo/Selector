using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic.Watcher.Consumer;
using Selector.Cache.Consumer.AppleMusic;
using StackExchange.Redis;

namespace Selector.Cache
{
    public interface IPublisherFactory
    {
        public Task<ISpotifyPlayerConsumer> GetSpotify(ISpotifyPlayerWatcher watcher = null);
        public Task<IApplePlayerConsumer> GetApple(IAppleMusicPlayerWatcher watcher = null);
    }

    public class PublisherFactory : IPublisherFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly ISubscriber Subscriber;

        public PublisherFactory(
            ISubscriber subscriber,
            ILoggerFactory loggerFactory
        )
        {
            Subscriber = subscriber;
            LoggerFactory = loggerFactory;
        }

        public Task<ISpotifyPlayerConsumer> GetSpotify(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult<ISpotifyPlayerConsumer>(new SpotifyPublisher(
                watcher,
                Subscriber,
                LoggerFactory.CreateLogger<SpotifyPublisher>()
            ));
        }

        public Task<IApplePlayerConsumer> GetApple(IAppleMusicPlayerWatcher watcher = null)
        {
            return Task.FromResult<IApplePlayerConsumer>(new ApplePublisher(
                watcher,
                Subscriber,
                LoggerFactory.CreateLogger<ApplePublisher>()
            ));
        }
    }
}