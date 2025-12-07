using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;
using Selector.Model.Extensions;

namespace Selector.CLI;

public static class ScrobbleSaveCommand
{
    public static async Task<int> Execute(DateTime from, DateTime to, int page, int delay, int simultaneous,
        string username, bool noAdd, bool noRemove, CancellationToken token)
    {
        try
        {
            var context = new CommandContext().WithLogger().WithDb().WithLastfmApi();
            var logger = context.Logger.CreateLogger("Scrobble");

            using var db = new ApplicationDbContext(context.DatabaseConfig.Options,
                context.Logger.CreateLogger<ApplicationDbContext>());
            var repo = new ScrobbleRepository(db);

            logger.LogInformation("Running from {} to {}", from, to);

            logger.LogInformation("Searching for {}", username);
            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.UserName == username);

            if (user is not null)
            {
                if (user.LastFmConnected())
                {
                    logger.LogInformation("Last.fm username found ({}), starting...", user.LastFmUsername);

                    if (from.Kind != DateTimeKind.Utc)
                    {
                        from = from.ToUniversalTime();
                    }

                    if (to.Kind != DateTimeKind.Utc)
                    {
                        to = to.ToUniversalTime();
                    }

                    await new ScrobbleSaver(
                            context.LastFmClient.User,
                            new()
                            {
                                User = user,
                                InterRequestDelay = new TimeSpan(0, 0, 0, 0, delay),
                                From = from,
                                To = to,
                                PageSize = page,
                                DontAdd = noAdd,
                                DontRemove = noRemove,
                                SimultaneousConnections = simultaneous
                            },
                            repo,
                            context.Logger.CreateLogger<ScrobbleSaver>(),
                            context.Logger)
                        .Execute(token);
                }
                else
                {
                    logger.LogError("{} doesn't have a Last.fm username", username);
                }
            }
            else
            {
                logger.LogError("{} not found", username);
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