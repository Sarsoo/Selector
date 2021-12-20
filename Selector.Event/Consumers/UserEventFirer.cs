using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Selector.Events;

namespace Selector
{
    public class UserEventFirer : IConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly ILogger<UserEventFirer> Logger;

        protected readonly UserEventBus UserEvent;

        public CancellationToken CancelToken { get; set; }

        public UserEventFirer(
            IPlayerWatcher watcher,
            UserEventBus userEvent,
            ILogger<UserEventFirer> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            UserEvent = userEvent;
            Logger = logger ?? NullLogger<UserEventFirer>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;
            
            Task.Run(async () => {
                try
                {
                    await AsyncCallback(e);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public Task AsyncCallback(ListeningChangeEventArgs e)
        {
            Logger.LogDebug("Firing now playing event on user bus [{username}/{userId}]", e.SpotifyUsername, e.Id);

            UserEvent.OnCurrentlyPlayingChange(this, e.Id, (CurrentlyPlayingDTO) e);

            return Task.CompletedTask;
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange += Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }
    }
}
