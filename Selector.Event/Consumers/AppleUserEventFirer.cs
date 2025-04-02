using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;

namespace Selector.Events
{
    public class AppleUserEventFirer(
        IAppleMusicPlayerWatcher watcher,
        UserEventBus userEvent,
        ILogger<AppleUserEventFirer> logger = null,
        CancellationToken token = default)
        : BaseSequentialPlayerConsumer<IAppleMusicPlayerWatcher, AppleListeningChangeEventArgs>(watcher, logger),
            IApplePlayerConsumer
    {
        protected readonly UserEventBus UserEvent = userEvent;

        public CancellationToken CancelToken { get; set; } = token;

        protected override Task ProcessEvent(AppleListeningChangeEventArgs e)
        {
            Logger.LogDebug("Firing Apple now playing event on user bus [{userId}]", e.Id);

            UserEvent.OnCurrentlyPlayingChangeApple(this, (AppleCurrentlyPlayingDTO)e);

            return Task.CompletedTask;
        }
    }
}