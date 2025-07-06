using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Selector.Operations;
using SpotifyAPI.Web;
using Trace = Selector.LastFm.Trace;

namespace Selector.Mapping
{
    public enum LastfmObject
    {
        Track,
        Album,
        Artist
    }

    /// <summary>
    /// Map Last.fm resources to Spotify resources using the Spotify search endpoint before saving mappings to database
    /// </summary>
    public abstract class ScrobbleMapping : IOperation
    {
        private readonly ILogger<ScrobbleMapping> _logger;
        private readonly ISearchClient _searchClient;

        public event EventHandler? Success;

        public int MaxAttempts { get; private set; } = 5;
        public int Attempts { get; private set; }

        private Task<SearchResponse>? CurrentTask { get; set; }
        public bool Succeeded { get; private set; } = false;

        public abstract object? Result { get; }
        public abstract string Query { get; }
        public abstract SearchRequest.Types QueryType { get; }

        private TaskCompletionSource AggregateTaskSource { get; set; } = new();
        public Task Task => AggregateTaskSource.Task;

        public ScrobbleMapping(ISearchClient searchClient, ILogger<ScrobbleMapping> logger)
        {
            _logger = logger;
            _searchClient = searchClient;
        }

        public Task Execute()
        {
            using var span = Trace.Tracer.StartActivity();

            _logger.LogInformation("Mapping Last.fm {} ({}) to Spotify", Query, QueryType);

            var netTime = Stopwatch.StartNew();
            CurrentTask = _searchClient.Item(new(QueryType, Query));
            CurrentTask.ContinueWith(async t =>
            {
                try
                {
                    netTime.Stop();
                    _logger.LogTrace("Network request took {:n} ms", netTime.ElapsedMilliseconds);

                    if (t.IsCompletedSuccessfully)
                    {
                        HandleResponse(t);
                        OnSuccess();
                        AggregateTaskSource.SetResult();
                    }
                    else
                    {
                        if (t.Exception?.InnerException is APITooManyRequestsException ex)
                        {
                            _logger.LogError("Spotify search request too many requests, waiting for {}", ex.RetryAfter);
                            await Task.Delay(ex.RetryAfter.Add(TimeSpan.FromSeconds(1)));
                            await Execute();
                        }
                        else
                        {
                            _logger.LogError("Spotify search request task faulted, {}", t.Exception);
                            if (t.Exception != null) AggregateTaskSource.SetException(t.Exception);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while mapping Last.fm {} ({}) to Spotify on attempt {}", Query,
                        QueryType,
                        Attempts);
                    Succeeded = false;
                }
            });

            Attempts++;
            return Task.CompletedTask;
        }

        public abstract void HandleResponse(Task<SearchResponse> response);

        protected virtual void OnSuccess()
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            Success?.Invoke(this, EventArgs.Empty);
        }
    }
}