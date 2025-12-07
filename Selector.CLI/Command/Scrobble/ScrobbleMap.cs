using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;

namespace Selector.CLI;

public static class ScrobbleMapCommand
{
    public static async Task<int> Execute(int delay, int simultaneous, int? limit, CancellationToken token)
    {
        try
        {
            var context = new CommandContext().WithLogger().WithDb().WithSpotify();
            var logger = context.Logger.CreateLogger("Scrobble");

            using var db = new ApplicationDbContext(context.DatabaseConfig.Options,
                context.Logger.CreateLogger<ApplicationDbContext>());

            await new ScrobbleMapper(
                    context.Spotify.Search,
                    new()
                    {
                        InterRequestDelay = new TimeSpan(0, 0, 0, 0, delay),
                        SimultaneousConnections = simultaneous,
                        Limit = limit
                    },
                    new ScrobbleRepository(db),
                    new ScrobbleMappingRepository(db),
                    context.Logger.CreateLogger<ScrobbleMapper>(),
                    context.Logger)
                .Execute(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }

        return 0;
    }
}