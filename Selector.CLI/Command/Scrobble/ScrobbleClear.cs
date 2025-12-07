using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;

namespace Selector.CLI;

public static class ScrobbleClearCommand
{
    public static async Task<int> Execute(DateTime? from, DateTime? to, string username, CancellationToken token)
    {
        try
        {
            var context = new CommandContext().WithLogger().WithDb();
            var logger = context.Logger.CreateLogger("Scrobble");

            var db = new ApplicationDbContext(context.DatabaseConfig.Options,
                context.Logger.CreateLogger<ApplicationDbContext>());

            logger.LogInformation("Searching for {0}", username);
            var user = db.Users
                .Include(u => u.Scrobbles)
                .FirstOrDefault(u => u.UserName == username);

            if (user is not null)
            {
                user.Scrobbles = user.Scrobbles
                    .Where(s => s.Timestamp < (from ?? DateTime.MinValue)
                                && s.Timestamp > (to ?? DateTime.MaxValue))
                    .ToList();

                await db.SaveChangesAsync();
            }
            else
            {
                logger.LogError("{0} not found", username);
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