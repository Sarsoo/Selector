using IF.Lastfm.Core.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selector.Model;
using Selector.Model.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.CLI.Services
{
    public class ScrobbleMonitor : IHostedService
    {
        private readonly ILogger<ScrobbleMonitor> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly ScrobbleMonitorOptions config;
        private readonly IUserApi userApi;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private Task task;

        public ScrobbleMonitor(ILogger<ScrobbleMonitor> _logger, IOptions<ScrobbleMonitorOptions> _options, IUserApi _userApi, IServiceScopeFactory _serviceScopeFactory, ILoggerFactory _loggerFactory)
        {
            logger = _logger;
            userApi = _userApi;
            config = _options.Value;
            serviceScopeFactory = _serviceScopeFactory;
            loggerFactory = _loggerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting scrobble monitor");

            using var scope = serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await RunScrobbleSavers(db, cancellationToken);
        }

        public async Task RunScrobbleSavers(ApplicationDbContext db, CancellationToken token)
        {
            foreach (var user in db.Users
                .AsNoTracking()
                .AsEnumerable()
                .Where(u => u.ScrobbleSavingEnabled()))
            {
                logger.LogInformation("Starting scrobble saver for {0}/{1}", user.UserName, user.LastFmUsername);

                await new ScrobbleSaver(userApi, new ScrobbleSaverConfig()
                {
                    User = user,
                    InterRequestDelay = config.InterRequestDelay,
                    From = DateTime.UtcNow.AddDays(-3)
                }, serviceScopeFactory, loggerFactory.CreateLogger<ScrobbleSaver>()).Execute(token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
