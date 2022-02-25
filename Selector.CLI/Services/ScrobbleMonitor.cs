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

        public Task RunScrobbleSavers(ApplicationDbContext db, CancellationToken token)
        {
            using var scope = serviceScopeFactory.CreateScope();

            foreach (var user in db.Users
                .AsNoTracking()
                .AsEnumerable()
                .Where(u => u.ScrobbleSavingEnabled()))
            {
                //TODO
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
