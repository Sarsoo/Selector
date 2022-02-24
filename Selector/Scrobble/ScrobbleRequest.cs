using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Selector.Operations;
using System;
using System.Collections.Generic;
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
            logger.LogInformation("Scrobble request #{} for {} by {} from {} to {}", pageNumber, username, pageSize, from, to);
            currentTask = userClient.GetRecentScrobbles(username, pagenumber: pageNumber, count: pageSize, from: from, to: to);
            currentTask.ContinueWith(t =>
            {
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
                        if(Attempts < MaxAttempts)
                        {
                            logger.LogDebug("Request failed for {}, #{} by {}: {}, retrying ({} of {})", username, pageNumber, pageSize, result.Status, Attempts + 1, MaxAttempts);
                            Execute();
                        }
                        else
                        {
                            logger.LogDebug("Request failed for {}, #{} by {}: {}, max retries exceeded {}, not retrying", username, pageNumber, pageSize, result.Status, MaxAttempts);
                            AggregateTaskSource.SetCanceled();
                        }
                    }
                }
                else
                {
                    logger.LogError("Scrobble request task faulted, {}", t.Exception);
                    AggregateTaskSource.SetException(t.Exception);
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
