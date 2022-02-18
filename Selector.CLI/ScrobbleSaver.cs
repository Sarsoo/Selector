using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Selector.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class ScrobbleSaverConfig
    {
        public ApplicationUser User { get; set; }
        public TimeSpan InterRequestDelay { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageSize { get; set; } = 100;
        public int Retries { get; set; } = 5;
        public bool DontAdd { get; set; } = false;
        public bool DontRemove { get; set; } = false;
    }

    public class ScrobbleSaver
    {
        private readonly ILogger<ScrobbleSaver> logger;

        private readonly IUserApi userClient;
        private readonly ScrobbleSaverConfig config;
        private readonly ApplicationDbContext db;

        public ScrobbleSaver(IUserApi _userClient, ScrobbleSaverConfig _config, ApplicationDbContext _db, ILogger<ScrobbleSaver> _logger)
        {
            userClient = _userClient;
            config = _config;
            db = _db;
            logger = _logger;
        }

        public async Task Execute(CancellationToken token)
        {
            logger.LogInformation("Saving scrobbles for {0}/{1}", config.User.UserName, config.User.LastFmUsername);

            var page1 = await userClient.GetRecentScrobbles(config.User.LastFmUsername, count: config.PageSize, from: config.From, to: config.To);

            if(page1.Success)
            {
                var scrobbles = page1.Content.ToList();

                if (page1.TotalPages > 1)
                {
                    var tasks = await GetScrobblesFromPageNumbers(2, page1.TotalPages, token);
                    var taskResults = await Task.WhenAll(tasks);

                    foreach (var result in taskResults)
                    {
                        if (result.Success)
                        {
                            scrobbles.AddRange(result.Content);
                        }
                        else
                        {
                            logger.LogWarning("Failed to get a subset of scrobbles for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
                        }
                    }                  
                }

                logger.LogDebug("Ordering and filtering pulled scrobbles");

                var nativeScrobbles = scrobbles
                    .DistinctBy(s => s.TimePlayed?.UtcDateTime)
                    .Select(s =>
                    {
                        var nativeScrobble = (UserScrobble) s;
                        nativeScrobble.UserId = config.User.Id;
                        return nativeScrobble;
                    });

                logger.LogDebug("Pulling currently stored scrobbles");

                var currentScrobbles = db.Scrobble
                    .AsEnumerable()
                    .Where(s => s.UserId == config.User.Id);

                if (config.From is not null)
                {
                    currentScrobbles = currentScrobbles.Where(s => s.Timestamp > config.From);
                }

                if (config.To is not null)
                {
                    currentScrobbles = currentScrobbles.Where(s => s.Timestamp < config.To);
                }

                logger.LogInformation("Completed scrobble pulling for {0}, pulled {1:n0}", config.User.UserName, nativeScrobbles.Count());

                logger.LogDebug("Identifying difference sets");
                var time = Stopwatch.StartNew();

                (var toAdd, var toRemove) = ScrobbleMatcher.IdentifyDiffs(currentScrobbles, nativeScrobbles);

                var toAddUser = toAdd.Cast<UserScrobble>().ToList();
                var toRemoveUser = toRemove.Cast<UserScrobble>().ToList();

                time.Stop();
                logger.LogTrace("Finished diffing: {0:n}ms", time.ElapsedMilliseconds);

                var timeDbOps = Stopwatch.StartNew();

                if(!config.DontAdd)
                {
                    await db.Scrobble.AddRangeAsync(toAddUser);
                }
                else
                {
                    logger.LogInformation("Skipping adding of {0} scrobbles", toAddUser.Count);
                }
                if (!config.DontRemove)
                {
                    db.Scrobble.RemoveRange(toRemoveUser);
                }
                else
                {
                    logger.LogInformation("Skipping removal of {0} scrobbles", toRemoveUser.Count);
                }
                await db.SaveChangesAsync();

                timeDbOps.Stop();
                logger.LogTrace("DB ops: {0:n}ms", timeDbOps.ElapsedMilliseconds);

                logger.LogInformation("Completed scrobble pulling for {0}, +{1:n0}, -{2:n0}", config.User.UserName, toAddUser.Count(), toRemoveUser.Count());
            }
            else
            {
                logger.LogError("Failed to pull first scrobble page for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
            }
        }

        private async Task<List<Task<PageResponse<LastTrack>>>> GetScrobblesFromPageNumbers(int start, int totalPages, CancellationToken token)
        {
            var tasks = new List<Task<PageResponse<LastTrack>>>();

            foreach (var pageNumber in Enumerable.Range(start, totalPages - 1))
            {
                logger.LogInformation("Pulling page {2:n0}/{3:n0} for {0}/{1}", config.User.UserName, config.User.LastFmUsername, pageNumber, totalPages);
                
                tasks.Add(userClient.GetRecentScrobbles(config.User.LastFmUsername, pagenumber: pageNumber, count: config.PageSize, from: config.From, to: config.To));
                await Task.Delay(config.InterRequestDelay, token);
            }

            return tasks;
        }
    }
}
