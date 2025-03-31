using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;

namespace Selector.Events
{
    public class AppleUserEventFirer : IApplePlayerConsumer
    {
        protected readonly IAppleMusicPlayerWatcher Watcher;
        protected readonly ILogger<AppleUserEventFirer> Logger;

        protected readonly UserEventBus UserEvent;

        public CancellationToken CancelToken { get; set; }

        public AppleUserEventFirer(
            IAppleMusicPlayerWatcher watcher,
            UserEventBus userEvent,
            ILogger<AppleUserEventFirer> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            UserEvent = userEvent;
            Logger = logger ?? NullLogger<AppleUserEventFirer>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, AppleListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            Task.Run(async () =>
            {
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

        public Task AsyncCallback(AppleListeningChangeEventArgs e)
        {
            Logger.LogDebug("Firing Apple now playing event on user bus [{userId}]", e.Id);

            UserEvent.OnCurrentlyPlayingChangeApple(this, e);

            return Task.CompletedTask;
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IAppleMusicPlayerWatcher watcherCast)
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

            if (watcher is IAppleMusicPlayerWatcher watcherCast)
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