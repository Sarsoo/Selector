using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.Model.Services
{
    public class MigratorService : IHostedService
    {
        private readonly IServiceScopeFactory scopeProvider;
        private readonly DatabaseOptions options;
        private readonly ILogger<MigratorService> logger;

        public MigratorService(IServiceScopeFactory _scopeProvider, IOptions<DatabaseOptions> _options, ILogger<MigratorService> _logger)
        {
            scopeProvider = _scopeProvider;
            options = _options.Value;
            logger = _logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(options.Migrate)
            {
                using var scope = scopeProvider.CreateScope();
                
                logger.LogInformation("Applying migrations");
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
