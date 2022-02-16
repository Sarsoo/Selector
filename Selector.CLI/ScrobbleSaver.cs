using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Selector.Model;
using System;
using System.Collections.Generic;
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
    }

    public class ScrobbleSaver
    {
        private readonly ILogger<ScrobbleSaver> logger;

        private readonly IUserApi userClient;
        private readonly ScrobbleSaverConfig config;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public ScrobbleSaver(IUserApi _userClient, ScrobbleSaverConfig _config, IServiceScopeFactory _serviceScopeFactory, ILogger<ScrobbleSaver> _logger)
        {
            userClient = _userClient;
            config = _config;
            serviceScopeFactory = _serviceScopeFactory;
            logger = _logger;
        }

        public async Task Execute(CancellationToken token)
        {
            logger.LogInformation("Saving all scrobbles for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
            
            using var scope = serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var page1 = await userClient.GetRecentScrobbles(config.User.LastFmUsername, count: config.PageSize, from: config.From, to: config.To);

            if(page1.Success)
            {
                var scrobbles = page1.Content.ToList();

                if(page1.TotalPages > 1)
                {
                    var tasks = await GetScrobblesFromPageNumbers(Enumerable.Range(2, page1.TotalPages - 1), token);
                    var taskResults = await Task.WhenAll(tasks);

                    foreach (var result in taskResults)
                    {
                        if (result.Success)
                        {
                            scrobbles.AddRange(result.Content);
                        }
                        else
                        {
                            logger.LogInformation("Failed to get a subset of scrobbles for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
                        }
                    }
                }

                var nativeScrobbles = scrobbles
                    .DistinctBy(s => s.TimePlayed)
                    .OrderBy(s => s.TimePlayed)
                    .Select(s =>
                    {
                        var nativeScrobble = (UserScrobble) s;
                        nativeScrobble.UserId = config.User.Id;
                        return nativeScrobble;
                    });
                
                var currentScrobbles = db.Scrobble
                    .AsEnumerable()
                    .OrderBy(s => s.Timestamp)
                    .Where(s => s.UserId == config.User.Id);

                if (config.From is not null)
                {
                    currentScrobbles = currentScrobbles.Where(s => s.Timestamp > config.From);
                }

                if (config.To is not null)
                {
                    currentScrobbles = currentScrobbles.Where(s => s.Timestamp < config.To);
                }

                (var toAdd, var toRemove) = ScrobbleMatcher.IdentifyDiffsContains(currentScrobbles, nativeScrobbles);

                await db.Scrobble.AddRangeAsync(toAdd.Cast<UserScrobble>());
                db.Scrobble.RemoveRange(toRemove.Cast<UserScrobble>());
                await db.SaveChangesAsync();
            }
            else
            {
                logger.LogError("Failed to pull first scrobble page for {0}/{1}", config.User.UserName, config.User.LastFmUsername);
            }
        }

        private async Task<List<Task<PageResponse<LastTrack>>>> GetScrobblesFromPageNumbers(IEnumerable<int> pageNumbers, CancellationToken token)
        {
            var tasks = new List<Task<PageResponse<LastTrack>>>();

            foreach (var pageNumber in pageNumbers)
            {
                logger.LogInformation("Pulling page {2} for {0}/{1}", config.User.UserName, config.User.LastFmUsername, pageNumber);
                
                tasks.Add(userClient.GetRecentScrobbles(config.User.LastFmUsername, pagenumber: pageNumber, count: config.PageSize, from: config.From, to: config.To));
                await Task.Delay(config.InterRequestDelay, token);
            }

            return tasks;
        }
    }
}
