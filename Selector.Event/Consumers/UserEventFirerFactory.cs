using Microsoft.Extensions.Logging;

namespace Selector.Events
{
    public interface IUserEventFirerFactory
    {
        public Task<UserEventFirer> Get(IPlayerWatcher watcher = null);
    }
    
    public class UserEventFirerFactory: IUserEventFirerFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly UserEventBus UserEvent;

        public UserEventFirerFactory(ILoggerFactory loggerFactory, UserEventBus userEvent)
        {
            LoggerFactory = loggerFactory;
            UserEvent = userEvent;
        }

        public Task<UserEventFirer> Get(IPlayerWatcher watcher = null)
        {
            return Task.FromResult(new UserEventFirer(
                watcher,
                UserEvent,
                LoggerFactory.CreateLogger<UserEventFirer>()
            ));
        }
    }
}
