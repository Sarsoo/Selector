using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Data;
using Selector.Model;
using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Collections.Generic;
using Selector.Cache;

namespace Selector.CLI
{
    public class SpotifyHistoryCommand : Command
    {
        public SpotifyHistoryCommand(string name, string description = null) : base(name, description)
        {
            var connectionString = new Option<string>("--connection", "database to migrate");
            connectionString.AddAlias("-c");
            AddOption(connectionString);

            var pathString = new Option<string>("--path", "path to find data");
            pathString.AddAlias("-i");
            AddOption(pathString);

            var username = new Option<string>("--username", "user to pulls scrobbles for");
            username.AddAlias("-u");
            AddOption(username);

            Handler = CommandHandler.Create((string connection, string path, string username) => Execute(connection, path, username));
        }

        public static int Execute(string connection, string path, string username)
        {
            var streams = new List<FileStream>();

            try
            {
                var context = new CommandContext().WithLogger().WithDb(connection).WithSpotify().WithRedis();
                var logger = context.Logger.CreateLogger("Scrobble");

                using var db = new ApplicationDbContext(context.DatabaseConfig.Options, context.Logger.CreateLogger<ApplicationDbContext>());

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

                foreach(var file in endSongs)
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
                foreach(var stream in streams)
                {
                    stream.Dispose();
                }
            }

            return 0;
        }
    }
}
