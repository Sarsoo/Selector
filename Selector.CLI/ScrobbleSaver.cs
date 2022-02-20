using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class ScrobbleSaverConfig
    {
        public ApplicationUser User { get; set; }
        public TimeSpan InterRequestDelay { get; set; }
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 20, 0);
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageSize { get; set; } = 100;
        public int Retries { get; set; } = 5;
        public int SimultaneousConnections { get; set; } = 3;
        public bool DontAdd { get; set; } = false;
        public bool DontRemove { get; set; } = false;
    }

    public class ScrobbleSaver
    {
        private readonly ILogger<ScrobbleSaver> logger;
        private readonly ILoggerFactory loggerFactory;

        private readonly IUserApi userClient;
        private readonly ScrobbleSaverConfig config;
        private CancellationToken _token;
        private Task aggregateNetworkTask;

        private readonly ApplicationDbContext db;

        private ConcurrentQueue<ScrobbleRequest> waitingRequests = new();
        private ConcurrentQueue<ScrobbleRequest> runRequests = new();

        public ScrobbleSaver(IUserApi _userClient, ScrobbleSaverConfig _config, ApplicationDbContext _db, ILogger<ScrobbleSaver> _logger, ILoggerFactory _loggerFactory = null)
        {
            userClient = _userClient;
            config = _config;
            db = _db;
            logger = _logger;
            loggerFactory = _loggerFactory;
        }

        public async Task Execute(CancellationToken token)
        {
            logger.LogInformation("Saving scrobbles for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
            _token = token;

            var page1 = new ScrobbleRequest(userClient, 
                loggerFactory?.CreateLogger<ScrobbleRequest>() ?? NullLogger<ScrobbleRequest>.Instance, 
                config.User.UserName, 
                1, 
                config.PageSize, 
                config.From, config.To);

            await page1.Execute();
            runRequests.Enqueue(page1);

            if (page1.Succeeded)
            {
                if (page1.TotalPages > 1)
                {
                    TriggerNetworkRequests(page1.TotalPages, token);
                }

                logger.LogDebug("Pulling currently stored scrobbles");

                var currentScrobblesPulled = GetDbScrobbles();

                await aggregateNetworkTask;
                var scrobbles = runRequests.SelectMany(r => r.Scrobbles);

                IdentifyDuplicates(scrobbles);

                logger.LogDebug("Ordering and filtering pulled scrobbles");

                RemoveNowPlaying(scrobbles.ToList());

                var nativeScrobbles = scrobbles
                    .DistinctBy(s => s.TimePlayed?.UtcDateTime)
                    .Select(s =>
                    {
                        var nativeScrobble = (UserScrobble)s;
                        nativeScrobble.UserId = config.User.Id;
                        return nativeScrobble;
                    });

                logger.LogInformation("Completed database scrobble pulling for {0}, pulled {1:n0}", config.User.UserName, nativeScrobbles.Count());

                logger.LogDebug("Identifying difference sets");
                var time = Stopwatch.StartNew();

                (var toAdd, var toRemove) = ScrobbleMatcher.IdentifyDiffs(currentScrobblesPulled, nativeScrobbles);

                time.Stop();
                logger.LogTrace("Finished diffing: {0:n}ms", time.ElapsedMilliseconds);

                var timeDbOps = Stopwatch.StartNew();

                if(!config.DontAdd)
                {
                    await db.Scrobble.AddRangeAsync(toAdd.Cast<UserScrobble>());
                }
                else
                {
                    logger.LogInformation("Skipping adding of {0} scrobbles", toAdd.Count());
                }
                if (!config.DontRemove)
                {
                    db.Scrobble.RemoveRange(toRemove.Cast<UserScrobble>());
                }
                else
                {
                    logger.LogInformation("Skipping removal of {0} scrobbles", toRemove.Count());
                }
                await db.SaveChangesAsync();

                timeDbOps.Stop();
                logger.LogTrace("DB ops: {0:n}ms", timeDbOps.ElapsedMilliseconds);

                logger.LogInformation("Completed scrobble pulling for {0}, +{1:n0}, -{2:n0}", config.User.UserName, toAdd.Count(), toRemove.Count());
            }
            else
            {
                logger.LogError("Failed to pull first scrobble page for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
            }
        }

        private async void HandleSuccessfulRequest(object o, EventArgs e)
        {
            await Task.Delay(config.InterRequestDelay, _token);
            TransitionRequest();
        }

        private void TransitionRequest()
        {
            if (waitingRequests.TryDequeue(out var request))
            {
                request.Success += HandleSuccessfulRequest;
                _ = request.Execute();
                runRequests.Enqueue(request);
            }
        }

        private void TriggerNetworkRequests(int totalPages, CancellationToken token)
        {
            foreach (var req in GetRequestsFromPageNumbers(2, totalPages))
            {
                waitingRequests.Enqueue(req);
            }

            foreach (var _ in Enumerable.Range(1, config.SimultaneousConnections))
            {
                TransitionRequest();
            }

            var timeoutTask = Task.Delay(config.Timeout, token);
            var allTasks = waitingRequests.Union(runRequests).Select(r => r.Task).ToList();

            aggregateNetworkTask = Task.WhenAny(timeoutTask, Task.WhenAll(allTasks));

            aggregateNetworkTask.ContinueWith(t =>
            {
                if (timeoutTask.IsCompleted)
                {
                    throw new TimeoutException($"Timed-out pulling scrobbles, took {config.Timeout}");
                }
            });            
        }

        private IEnumerable<UserScrobble> GetDbScrobbles()
        {
            var currentScrobbles = db.Scrobble.AsEnumerable()
                    .Where(s => s.UserId == config.User.Id);

            if (config.From is not null)
            {
                currentScrobbles = currentScrobbles.Where(s => s.Timestamp > config.From);
            }

            if (config.To is not null)
            {
                currentScrobbles = currentScrobbles.Where(s => s.Timestamp < config.To);
            }

            return currentScrobbles;
        }

        private IEnumerable<ScrobbleRequest> GetRequestsFromPageNumbers(int start, int totalPages)
            => Enumerable.Range(start, totalPages - 1)
                         .Select(n => new ScrobbleRequest(
                             userClient, 
                             loggerFactory.CreateLogger<ScrobbleRequest>() ?? NullLogger<ScrobbleRequest>.Instance, 
                             config.User.UserName, 
                             n, 
                             config.PageSize, 
                             config.From, 
                             config.To));

        private void IdentifyDuplicates(IEnumerable<LastTrack> tracks)
        {
            logger.LogDebug("Identifying duplicates");

            var duplicates = tracks
                .GroupBy(t => t.TimePlayed?.UtcDateTime)
                .Where(g => g.Count() > 1);

            foreach(var dupe in duplicates)
            {
                var dupeString = new StringBuilder();

                foreach(var scrobble in dupe)
                {
                    dupeString.Append("(");
                    dupeString.Append(scrobble.Name);
                    dupeString.Append(", ");
                    dupeString.Append(scrobble.AlbumName); 
                    dupeString.Append(", ");
                    dupeString.Append(scrobble.ArtistName);
                    dupeString.Append(")");
                    
                    dupeString.Append(" ");
                }

                logger.LogInformation("Duplicate at {0}: {1}", dupe.Key, dupeString.ToString());
            }
        }

        private bool RemoveNowPlaying(List<LastTrack> scrobbles)
        {
            var newestScrobble = scrobbles.FirstOrDefault();
            if (newestScrobble is not null)
            {
                if (newestScrobble.IsNowPlaying is bool playing && playing)
                {
                    scrobbles.Remove(newestScrobble);
                    return true;
                }
            }

            return false;
        }
    }
}
