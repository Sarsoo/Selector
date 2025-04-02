using Microsoft.Extensions.Logging;
using Selector.Spotify;
using Selector.Spotify.Consumer;

namespace Selector.Events
{
    public class SpotifyUserEventFirer(
        ISpotifyPlayerWatcher watcher,
        UserEventBus userEvent,
        ILogger<SpotifyUserEventFirer> logger = null,
        CancellationToken token = default)
        : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>(watcher, logger),
            ISpotifyPlayerConsumer
    {
        protected readonly UserEventBus UserEvent = userEvent;

        public CancellationToken CancelToken { get; set; } = token;

        protected override Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return Task.CompletedTask;

            Logger.LogDebug("Firing Spotify now playing event on user bus [{username}/{userId}]", e.SpotifyUsername,
                e.Id);

            UserEvent.OnCurrentlyPlayingChangeSpotify(this, (SpotifyCurrentlyPlayingDTO)e);

            return Task.CompletedTask;
        }
    }
}