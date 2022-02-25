using Microsoft.Extensions.Logging;
using Selector.Operations;
using SpotifyAPI.Web;
using System;
using System.Threading.Tasks;

namespace Selector
{
    public enum LastfmObject{
        Track, Album, Artist
    }

    public abstract class ScrobbleMapping : IOperation
    {
        private readonly ILogger<ScrobbleMapping> logger;
        private readonly ISearchClient searchClient;

        public event EventHandler Success;

        public int MaxAttempts { get; private set; } = 5;
        public int Attempts { get; private set; }

        private Task<SearchResponse> currentTask { get; set; }
        public bool Succeeded { get; private set; } = false;

        public abstract object Result { get; }
        public abstract string Query { get; }
        public abstract SearchRequest.Types QueryType { get; }

        private TaskCompletionSource AggregateTaskSource { get; set; } = new();
        public Task Task => AggregateTaskSource.Task;

        public ScrobbleMapping(ISearchClient _searchClient, ILogger<ScrobbleMapping> _logger)
        {
            logger = _logger;
            searchClient = _searchClient;
        }

        public Task Execute()
        {
            logger.LogInformation("Mapping Last.fm {} ({}) to Spotify", Query, QueryType);

            currentTask = searchClient.Item(new (QueryType, Query));
            currentTask.ContinueWith(async t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    HandleResponse(t);
                    OnSuccess();
                    AggregateTaskSource.SetResult();
                }
                else
                {
                    if(t.Exception.InnerException is APITooManyRequestsException ex)
                    {
                        logger.LogError("Spotify search request too many requests, waiting for {}", ex.RetryAfter);
                        await Task.Delay(ex.RetryAfter.Add(TimeSpan.FromSeconds(1)));
                        await Execute();
                    }
                    else
                    {
                        logger.LogError("Spotify search request task faulted, {}", t.Exception);
                        AggregateTaskSource.SetException(t.Exception);
                    }
                }
            });

            Attempts++;
            return Task.CompletedTask;
        }

        public abstract void HandleResponse(Task<SearchResponse> response);

        protected virtual void OnSuccess()
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            Success?.Invoke(this, new EventArgs());
        }
    }
}
