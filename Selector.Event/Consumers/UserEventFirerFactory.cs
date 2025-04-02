using Microsoft.Extensions.Logging;
using Selector.Spotify;

namespace Selector.Events
{
    public interface IUserEventFirerFactory
    {
        public Task<SpotifyUserEventFirer> GetSpotify(ISpotifyPlayerWatcher watcher = null);
        public Task<AppleUserEventFirer> GetApple(IAppleMusicPlayerWatcher watcher = null);
    }

    public class UserEventFirerFactory : IUserEventFirerFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly UserEventBus UserEvent;

        public UserEventFirerFactory(ILoggerFactory loggerFactory, UserEventBus userEvent)
        {
            LoggerFactory = loggerFactory;
            UserEvent = userEvent;
        }

        public Task<SpotifyUserEventFirer> GetSpotify(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult(new SpotifyUserEventFirer(
                watcher,
                UserEvent,
                LoggerFactory.CreateLogger<SpotifyUserEventFirer>()
            ));
        }

        public Task<AppleUserEventFirer> GetApple(IAppleMusicPlayerWatcher watcher = null)
        {
            return Task.FromResult(new AppleUserEventFirer(
                watcher,
                UserEvent,
                LoggerFactory.CreateLogger<AppleUserEventFirer>()
            ));
        }
    }
}