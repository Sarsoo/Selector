using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Selector.Operations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Selector
{
    public class ScrobbleRequest : IOperation
    {
        private readonly ILogger<ScrobbleRequest> logger;
        private readonly IUserApi userClient;

        public event EventHandler Success;

        public int MaxAttempts { get; private set; }
        public int Attempts { get; private set; }
        public IEnumerable<LastTrack> Scrobbles { get; private set; }
        public int TotalPages { get; private set; }
        private Task<PageResponse<LastTrack>> currentTask { get; set; }
        public bool Succeeded { get; private set; } = false;

        private string username { get; set; }
        private int pageNumber { get; set; }
        int pageSize { get; set; }
        DateTime? from { get; set; }
        DateTime? to { get; set; }

        private TaskCompletionSource AggregateTaskSource { get; set; } = new();
        public Task Task => AggregateTaskSource.Task;

        public ScrobbleRequest(IUserApi _userClient, ILogger<ScrobbleRequest> _logger, string _username, int _pageNumber, int _pageSize, DateTime? _from, DateTime? _to, int maxRetries = 5)
        {
            userClient = _userClient;
            logger = _logger;

            username = _username;
            pageNumber = _pageNumber;
            pageSize = _pageSize;
            from = _from;
            to = _to;

            MaxAttempts = maxRetries;
        }

        public Task Execute()
        {
            using var scope = logger.BeginScope(new Dictionary<string, object>() { { "username", username }, { "page_number", pageNumber }, { "page_size", pageSize }, { "from", from }, { "to", to } });

            logger.LogInformation("Starting request");

            var netTime = Stopwatch.StartNew();
            currentTask = userClient.GetRecentScrobbles(username, pagenumber: pageNumber, count: pageSize, from: from, to: to);
            currentTask.ContinueWith(async t =>
            {
                using var scope = logger.BeginScope(new Dictionary<string, object>() { { "username", username }, { "page_number", pageNumber }, { "page_size", pageSize }, { "from", from }, { "to", to } });

                try
                {
                    netTime.Stop();
                    logger.LogTrace("Network request took {:n} ms", netTime.ElapsedMilliseconds);

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
                                logger.LogDebug("Request failed: {}, retrying ({} of {})", result.Status, Attempts + 1, MaxAttempts);
                                await Execute();
                            }
                            else
                            {
                                logger.LogDebug("Request failed: {}, max retries exceeded {}, not retrying", result.Status, MaxAttempts);
                                AggregateTaskSource.SetCanceled();
                            }
                        }
                    }
                    else
                    {
                        logger.LogError("Scrobble request task faulted, {}", t.Exception);
                        AggregateTaskSource.SetException(t.Exception);
                    }
                }
                catch(Exception e)
                {
                    logger.LogError(e, "Error while making scrobble request on attempt {}", Attempts);
                    Succeeded = false;
                }
            });

            Attempts++;
            return Task;
        }

        protected virtual void OnSuccess()
        {
            Success?.Invoke(this, new EventArgs());
        }
    }
}
