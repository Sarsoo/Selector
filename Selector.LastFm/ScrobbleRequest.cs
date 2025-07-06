using System.Diagnostics;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Selector.Operations;
using Trace = Selector.LastFm.Trace;

namespace Selector
{
    public class ScrobbleRequest : IOperation
    {
        private readonly ILogger<ScrobbleRequest> _logger;
        private readonly IUserApi _userClient;

        public event EventHandler? Success;

        public int MaxAttempts { get; private set; }
        public int Attempts { get; private set; }
        public IEnumerable<LastTrack> Scrobbles { get; private set; } = [];
        public int TotalPages { get; private set; }
        private Task<PageResponse<LastTrack>>? CurrentTask { get; set; }
        public bool Succeeded { get; private set; } = false;

        private string Username { get; set; }
        private int PageNumber { get; set; }
        private int PageSize { get; set; }
        private DateTime? From { get; set; }
        private DateTime? To { get; set; }

        private TaskCompletionSource AggregateTaskSource { get; set; } = new();
        public Task Task => AggregateTaskSource.Task;

        public ScrobbleRequest(IUserApi userClient, ILogger<ScrobbleRequest> logger, string username,
            int pageNumber, int pageSize, DateTime? from, DateTime? to, int maxRetries = 5)
        {
            _userClient = userClient;
            _logger = logger;

            Username = username;
            PageNumber = pageNumber;
            PageSize = pageSize;
            From = from;
            To = to;

            MaxAttempts = maxRetries;
        }

        public Task Execute()
        {
            using var span = Trace.Tracer.StartActivity();
            span?.AddBaggage(TraceConst.LastFmUsername, Username);
            span?.AddTag("page_number", PageNumber.ToString());

            using var scope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { TraceConst.LastFmUsername, Username }, { "page_number", PageNumber }, { "page_size", PageSize },
                { "from", From ?? DateTime.MinValue },
                { "to", To ?? DateTime.MinValue }
            });

            _logger.LogInformation("Starting request");

            var netTime = Stopwatch.StartNew();
            CurrentTask =
                _userClient.GetRecentScrobbles(Username, pagenumber: PageNumber, count: PageSize, from: From, to: To);
            CurrentTask.ContinueWith(async t =>
            {
                using var subspan = Trace.Tracer.StartActivity();
                subspan?.AddTag("page_number", PageNumber.ToString());

                using var scope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { TraceConst.LastFmUsername, Username }, { "page_number", PageNumber }, { "page_size", PageSize },
                    { "from", From ?? DateTime.MinValue }, { "to", To ?? DateTime.MinValue }
                });

                try
                {
                    netTime.Stop();
                    _logger.LogTrace("Network request took {:n} ms", netTime.ElapsedMilliseconds);

                    if (t.IsCompletedSuccessfully)
                    {
                        var result = t.Result;
                        Succeeded = result.Success;

                        if (Succeeded)
                        {
                            Scrobbles = result.Content.ToArray();
                            TotalPages = result.TotalPages;
                            OnSuccess();
                            AggregateTaskSource.SetResult();
                        }
                        else
                        {
                            if (Attempts < MaxAttempts)
                            {
                                _logger.LogDebug("Request failed: {}, retrying ({} of {})", result.Status, Attempts + 1,
                                    MaxAttempts);
                                await Execute();
                            }
                            else
                            {
                                _logger.LogDebug("Request failed: {}, max retries exceeded {}, not retrying",
                                    result.Status, MaxAttempts);
                                AggregateTaskSource.SetCanceled();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Scrobble request task faulted, {}", t.Exception);
                        if (t.Exception != null) AggregateTaskSource.SetException(t.Exception);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while making scrobble request on attempt {}", Attempts);
                    Succeeded = false;
                }
            });

            Attempts++;
            return Task;
        }

        protected virtual void OnSuccess()
        {
            Success?.Invoke(this, EventArgs.Empty);
        }
    }
}