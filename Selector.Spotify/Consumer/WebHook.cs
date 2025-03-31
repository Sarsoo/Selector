using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Spotify.Timeline;

namespace Selector.Spotify.Consumer
{
    public class WebHookConfig
    {
        public string Name { get; set; }
        public IEnumerable<Predicate<SpotifyListeningChangeEventArgs>> Predicates { get; set; }
        public string Url { get; set; }
        public HttpContent Content { get; set; }

        public bool ShouldRequest(SpotifyListeningChangeEventArgs e)
        {
            if (Predicates is not null)
            {
                return Predicates.Select(p => p(e)).Aggregate((a, b) => a && b);
            }
            else
            {
                return true;
            }
        }
    }

    public class WebHook : ISpotifyPlayerConsumer
    {
        protected readonly ISpotifyPlayerWatcher Watcher;
        protected readonly HttpClient HttpClient;
        protected readonly ILogger<WebHook> Logger;

        protected readonly WebHookConfig Config;

        public event EventHandler PredicatePass;
        public event EventHandler SuccessfulRequest;
        public event EventHandler FailedRequest;

        public CancellationToken CancelToken { get; set; }

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        public WebHook(
            ISpotifyPlayerWatcher watcher,
            HttpClient httpClient,
            WebHookConfig config,
            ILogger<WebHook> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            HttpClient = httpClient;
            Config = config;
            Logger = logger ?? NullLogger<WebHook>.Instance;
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

        public async Task AsyncCallback(SpotifyListeningChangeEventArgs e)
        {
            using var scope = Logger.BeginScope(new Dictionary<string, object>()
            {
                { "spotify_username", e.SpotifyUsername }, { "id", e.Id }, { "name", Config.Name },
                { "url", Config.Url }
            });

            if (Config.ShouldRequest(e))
            {
                try
                {
                    Logger.LogDebug("Predicate passed, making request");
                    OnPredicatePass(new EventArgs());

                    var response = await HttpClient.PostAsync(Config.Url, Config.Content, CancelToken);

                    if (response.IsSuccessStatusCode)
                    {
                        Logger.LogDebug("Request success");
                        OnSuccessfulRequest(new EventArgs());
                    }
                    else
                    {
                        Logger.LogDebug("Request failed [{error}] [{content}]", response.StatusCode, response.Content);
                        OnFailedRequest(new EventArgs());
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError(ex, "Exception occured during request");
                }
                catch (TaskCanceledException)
                {
                    Logger.LogDebug("Task cancelled");
                }
            }
            else
            {
                Logger.LogTrace("Predicate failed, skipping");
            }
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

        protected virtual void OnPredicatePass(EventArgs args)
        {
            PredicatePass?.Invoke(this, args);
        }

        protected virtual void OnSuccessfulRequest(EventArgs args)
        {
            SuccessfulRequest?.Invoke(this, args);
        }

        protected virtual void OnFailedRequest(EventArgs args)
        {
            FailedRequest?.Invoke(this, args);
        }
    }
}