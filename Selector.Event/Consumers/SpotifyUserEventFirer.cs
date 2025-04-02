using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Spotify;
using Selector.Spotify.Consumer;

namespace Selector.Events
{
    public class SpotifyUserEventFirer : ISpotifyPlayerConsumer
    {
        protected readonly ISpotifyPlayerWatcher Watcher;
        protected readonly ILogger<SpotifyUserEventFirer> Logger;

        protected readonly UserEventBus UserEvent;

        public CancellationToken CancelToken { get; set; }

        public SpotifyUserEventFirer(
            ISpotifyPlayerWatcher watcher,
            UserEventBus userEvent,
            ILogger<SpotifyUserEventFirer> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            UserEvent = userEvent;
            Logger = logger ?? NullLogger<SpotifyUserEventFirer>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, SpotifyListeningChangeEventArgs e)
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

        public Task AsyncCallback(SpotifyListeningChangeEventArgs e)
        {
            Logger.LogDebug("Firing Spotify now playing event on user bus [{username}/{userId}]", e.SpotifyUsername,
                e.Id);

            UserEvent.OnCurrentlyPlayingChangeSpotify(this, (SpotifyCurrentlyPlayingDTO)e);

            return Task.CompletedTask;
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is ISpotifyPlayerWatcher watcherCast)
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

            if (watcher is ISpotifyPlayerWatcher watcherCast)
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