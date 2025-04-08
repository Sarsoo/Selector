using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Selector.Model.Services
{
    public class MigratorService(
        IServiceScopeFactory scopeProvider,
        IOptions<DatabaseOptions> options,
        ILogger<MigratorService> logger)
        : IHostedService
    {
        private readonly DatabaseOptions _options = options.Value;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Migrate)
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