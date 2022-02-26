using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Selector.Model;
using Selector.Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.CLI.Jobs
{
    [DisallowConcurrentExecution]
    public class ScrobbleWatcherJob : IJob
    {
        private readonly ILogger<ScrobbleWatcherJob> logger;
        private readonly ILoggerFactory loggerFactory;

        private readonly IUserApi userApi;
        private readonly IScrobbleRepository scrobbleRepo;
        private readonly ApplicationDbContext db;
        private readonly ScrobbleWatcherJobOptions options;

        private static object databaseLock = new();

        public bool IsFull { get; set; }

        public ScrobbleWatcherJob(
            IUserApi _userApi,
            IScrobbleRepository _scrobbleRepo,
            ApplicationDbContext _db,
            IOptions<ScrobbleWatcherJobOptions> _options,
            ILogger<ScrobbleWatcherJob> _logger, 
            ILoggerFactory _loggerFactory)
        {
            logger = _logger;
            loggerFactory = _loggerFactory;

            userApi = _userApi;
            scrobbleRepo = _scrobbleRepo;
            db = _db;
            options = _options.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.LogInformation("Starting scrobble watching job");

                var users = db.Users
                    .AsEnumerable()
                    .Where(u => u.ScrobbleSavingEnabled())
                    .ToArray();

                foreach (var user in users)
                {
                    logger.LogInformation("Saving scrobbles for {}/{}", user.UserName, user.LastFmUsername);

                    DateTime? from = null;
                    if (options.From is not null && !IsFull)
                    {
                        from = options.From.Value.ToUniversalTime();
                    }

                    var saver = new ScrobbleSaver(
                        userApi,
                        new()
                        {
                            User = user,
                            InterRequestDelay = options.InterRequestDelay,
                            From = from,
                            PageSize = options.PageSize,
                            DontAdd = false,
                            DontRemove = false,
                            SimultaneousConnections = options.Simultaneous
                        },
                        scrobbleRepo,
                        loggerFactory.CreateLogger<ScrobbleSaver>(),
                        loggerFactory,
                        databaseLock);

                    await saver.Execute(context.CancellationToken);

                    logger.LogInformation("Finished scrobbles for {}/{}", user.UserName, user.LastFmUsername);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occured while saving scrobbles");
            }
        }
    }
}
