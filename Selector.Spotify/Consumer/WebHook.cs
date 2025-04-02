using Microsoft.Extensions.Logging;
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

    public class WebHook(
        ISpotifyPlayerWatcher watcher,
        HttpClient httpClient,
        WebHookConfig config,
        ILogger<WebHook> logger = null,
        CancellationToken token = default)
        : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>(watcher, logger),
            ISpotifyPlayerConsumer
    {
        protected readonly HttpClient HttpClient = httpClient;

        protected readonly WebHookConfig Config = config;

        public event EventHandler PredicatePass;
        public event EventHandler SuccessfulRequest;
        public event EventHandler FailedRequest;

        public CancellationToken CancelToken { get; set; } = token;

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

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