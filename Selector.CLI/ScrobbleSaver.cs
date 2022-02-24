using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using Selector.Operations;
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
        private Task batchTask;
        private BatchingOperation<ScrobbleRequest> batchOperation;

        private readonly IScrobbleRepository scrobbleRepo;

        public ScrobbleSaver(IUserApi _userClient, ScrobbleSaverConfig _config, IScrobbleRepository _scrobbleRepository, ILogger<ScrobbleSaver> _logger, ILoggerFactory _loggerFactory = null)
        {
            userClient = _userClient;
            config = _config;
            scrobbleRepo = _scrobbleRepository;
            logger = _logger;
            loggerFactory = _loggerFactory;
        }

        public async Task Execute(CancellationToken token)
        {
            logger.LogInformation("Saving scrobbles for {}/{}", config.User.UserName, config.User.LastFmUsername);

            var page1 = new ScrobbleRequest(userClient, 
                loggerFactory?.CreateLogger<ScrobbleRequest>() ?? NullLogger<ScrobbleRequest>.Instance, 
                config.User.UserName, 
                1, 
                config.PageSize, 
                config.From, config.To);

            await page1.Execute();

            if (page1.Succeeded)
            {
                if (page1.TotalPages > 1)
                {
                    batchOperation = new BatchingOperation<ScrobbleRequest>(
                        config.InterRequestDelay, 
                        config.Timeout,
                        config.SimultaneousConnections, 
                        GetRequestsFromPageNumbers(2, page1.TotalPages)
                    );

                    batchTask = batchOperation.TriggerRequests(token);
                }


                logger.LogDebug("Pulling currently stored scrobbles");

                var currentScrobbles = scrobbleRepo.GetAll(userId: config.User.Id, from: config.From, to: config.To);

                if(batchTask is not null)
                {
                    await batchTask;
                }

                IEnumerable<LastTrack> scrobbles;
                if(batchOperation is not null)
                {
                    scrobbles = page1.Scrobbles.Union(batchOperation.DoneRequests.SelectMany(r => r.Scrobbles));
                }
                else
                {
                    scrobbles = page1.Scrobbles;
                }

                logger.LogDebug("Ordering and filtering pulled scrobbles");

                scrobbles = scrobbles.Where(s => !(s.IsNowPlaying is bool playing && playing));

                IdentifyDuplicates(scrobbles);

                var nativeScrobbles = scrobbles
                    .DistinctBy(s => new { s.TimePlayed?.UtcDateTime, s.Name, s.ArtistName })
                    .Select(s => (UserScrobble) s)
                    .ToArray();

                logger.LogInformation("Completed database scrobble pulling for {}, pulled {:n0}", config.User.UserName, nativeScrobbles.Length);

                logger.LogDebug("Identifying difference sets");
                var time = Stopwatch.StartNew();

                (var toAdd, var toRemove) = ScrobbleMatcher.IdentifyDiffs(currentScrobbles, nativeScrobbles);

                time.Stop();
                logger.LogTrace("Finished diffing: {:n}ms", time.ElapsedMilliseconds);

                var timeDbOps = Stopwatch.StartNew();

                if(!config.DontAdd)
                {
                    foreach(var add in toAdd)
                    {
                        var scrobble = (UserScrobble) add;
                        scrobble.UserId = config.User.Id;
                        scrobbleRepo.Add(scrobble);
                    }
                }
                else
                {
                    logger.LogInformation("Skipping adding of {} scrobbles", toAdd.Count());
                }
                if (!config.DontRemove)
                {
                    foreach (var remove in toRemove)
                    {
                        var scrobble = (UserScrobble) remove;
                        scrobble.UserId = config.User.Id;
                        scrobbleRepo.Remove(scrobble);
                    }
                }
                else
                {
                    logger.LogInformation("Skipping removal of {} scrobbles", toRemove.Count());
                }
                await scrobbleRepo.Save();

                timeDbOps.Stop();
                logger.LogTrace("DB ops: {:n}ms", timeDbOps.ElapsedMilliseconds);

                logger.LogInformation("Completed scrobble pulling for {}, +{:n0}, -{:n0}", config.User.UserName, toAdd.Count(), toRemove.Count());
            }
            else
            {
                logger.LogError("Failed to pull first scrobble page for {}/{}", config.User.UserName, config.User.LastFmUsername);
            }
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
                             config.To,
                             config.Retries));

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
                    dupeString.Append('(');
                    dupeString.Append(scrobble.Name);
                    dupeString.Append(", ");
                    dupeString.Append(scrobble.AlbumName); 
                    dupeString.Append(", ");
                    dupeString.Append(scrobble.ArtistName);
                    dupeString.Append(") ");
                }

                logger.LogInformation("Duplicate at {}: {}", dupe.Key, dupeString.ToString());
            }
        }

        private IEnumerable<LastTrack> RemoveNowPlaying(IEnumerable<LastTrack> scrobbles)
        {
            var newestScrobble = scrobbles.FirstOrDefault();
            if (newestScrobble is not null)
            {
                if (newestScrobble.IsNowPlaying is bool playing && playing)
                {
                    scrobbles = scrobbles.Skip(1);
                }
            }

            return scrobbles;
        }
    }
}
