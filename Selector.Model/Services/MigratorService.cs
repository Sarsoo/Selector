using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.Model.Services
{
    public class MigratorService : IHostedService
    {
        private readonly ApplicationDbContext context;
        private readonly DatabaseOptions options;
        private readonly ILogger<MigratorService> logger;

        public MigratorService(ApplicationDbContext _context, IOptions<DatabaseOptions> _options, ILogger<MigratorService> _logger)
        {
            context = _context;
            options = _options.Value;
            logger = _logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(options.Migrate)
            {
                logger.LogInformation("Applying migrations");
                context.Database.Migrate();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
