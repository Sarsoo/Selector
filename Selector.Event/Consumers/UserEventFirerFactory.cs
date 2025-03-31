using Microsoft.Extensions.Logging;
using Selector.Spotify;

namespace Selector.Events
{
    public interface IUserEventFirerFactory
    {
        public Task<SpotifyUserEventFirer> Get(ISpotifyPlayerWatcher watcher = null);
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

        public Task<SpotifyUserEventFirer> Get(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult(new SpotifyUserEventFirer(
                watcher,
                UserEvent,
                LoggerFactory.CreateLogger<SpotifyUserEventFirer>()
            ));
        }
    }
}