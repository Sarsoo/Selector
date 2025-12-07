using Microsoft.Extensions.Logging;
using Selector.Cache;
using Selector.CLI.Extensions;
using Selector.Data;
using Selector.Model;

namespace Selector.CLI;

public static class SpotifyHistoryCommand
{
    public static int Execute(string connection, string path, string username)
    {
        var streams = new List<FileStream>();

        try
        {
            var context = new CommandContext().WithLogger().WithDb(connection).WithSpotify().WithRedis();
            var logger = context.Logger.CreateLogger("Scrobble");

            using var db = new ApplicationDbContext(context.DatabaseConfig.Options,
                context.Logger.CreateLogger<ApplicationDbContext>());

            var historyPersister = new HistoryPersister(db, new DataJsonContext(), new()
                {
                    Username = username,
                    Apply50PercentRule = true
                },
                durationPuller: new(context.Logger.CreateLogger<DurationPuller>(),
                    context.Spotify.Tracks,
                    cache: context.RedisMux.GetDatabase()),
                logger: context.Logger.CreateLogger<HistoryPersister>());

            logger.LogInformation("Preparing to parse from {} for {}", path, username);

            var directoryContents = Directory.EnumerateFiles(path);
            var endSongs = directoryContents.Where(f => f.Contains("endsong_")).ToArray();

            foreach (var file in endSongs)
            {
                streams.Add(File.OpenRead(file));
            }

            Console.WriteLine("Parse {0} historical data files? (y/n) ", endSongs.Length);
            var input = Console.ReadLine();

            if (input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Parsing files");

                historyPersister.Process(streams).Wait();
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
        finally
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }

        return 0;
    }
}