#if !AOT
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;

namespace Selector.CLI;

public static class MigrateCommand
{
    public static int Execute(string connectionString)
    {
        try
        {
            var context = new CommandContext().WithLogger().WithDb(connectionString).WithLastfmApi();
            var logger = context.Logger.CreateLogger("Scrobble");

            using var db = new ApplicationDbContext(context.DatabaseConfig.Options,
                context.Logger.CreateLogger<ApplicationDbContext>());

            logger.LogInformation("Preparing to migrate ({})", db.Database.GetConnectionString());

            Console.WriteLine("Migrate database? (y/n) ");
            var input = Console.ReadLine();

            if (input?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                logger.LogInformation("Migrating database");
                db.Database.Migrate();
            }
            else
            {
                logger.LogInformation("Exiting");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }

        return 0;
    }
}
#endif