using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Events;

namespace Selector
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
