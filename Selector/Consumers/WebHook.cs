using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector
{
    public class WebHookConfig
    {
        public string Name { get; set; }
        public IEnumerable<Predicate<ListeningChangeEventArgs>> Predicates { get; set; }
        public string Url { get; set; }
        public HttpContent Content { get; set; }

        public bool ShouldRequest(ListeningChangeEventArgs e)
        {
            if(Predicates is not null)
            {
                return Predicates.Select(p => p(e)).Aggregate((a, b) => a && b);
            }
            else
            {
                return true;
            }
        }
    }

    public class WebHook : IConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly HttpClient HttpClient;
        protected readonly ILogger<WebHook> Logger;

        protected readonly WebHookConfig Config;

        protected event EventHandler PredicatePass;
        protected event EventHandler SuccessfulRequest;
        protected event EventHandler FailedRequest;

        public CancellationToken CancelToken { get; set; }

        public AnalysedTrackTimeline Timeline { get; set; } = new();

        public WebHook(
            IPlayerWatcher watcher,
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

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            if(Config.ShouldRequest(e))
            {
                Logger.LogDebug("[{name}] predicate passed, making request to [{url}]", Config.Name, Config.Url);
                var response = await HttpClient.PostAsync(Config.Url, Config.Content, CancelToken);

                OnPredicatePass(new EventArgs());

                if (response.IsSuccessStatusCode)
                {
                    Logger.LogDebug("[{name}] request success", Config.Name);
                    OnSuccessfulRequest(new EventArgs());
                }
                else
                {
                    Logger.LogDebug("[{name}] request failed [{error}] [{content}]", Config.Name, response.StatusCode, response.Content);
                    OnFailedRequest(new EventArgs());
                }
            }
            else
            {
                Logger.LogTrace("[{name}] predicate failed, skipping", Config.Name);
            }
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
