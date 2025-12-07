using System.CommandLine;

namespace Selector.CLI.Extensions;

public static class CommandExtensions
{
    public static Command AddScrobbleCommand(this Command command)
    {
        Command scrobbleCmd = new("scrobble", "Manipulate scrobbles");
        command.Subcommands.Add(scrobbleCmd);

        scrobbleCmd.AddScrobbleMap()
            .AddScrobbleClear()
            .AddScrobbleSave();

        return command;
    }

    public static Command AddScrobbleSave(this Command command)
    {
        Option<DateTime> fromOption = new("--from")
        {
            Description = "Date from which to pull scrobbles",
            DefaultValueFactory = _ => DateTime.UtcNow.AddDays(-7)
        };

        Option<DateTime> toOption = new("--to")
        {
            Description = "Last date for which to pull scrobbles",
            DefaultValueFactory = _ => DateTime.UtcNow.AddHours(1)
        };

        Option<int> pageOption = new("--page", "-p")
        {
            Description = "Number of scrobbles per page",
            DefaultValueFactory = _ => 100
        };

        Option<int> delayOption = new("--delay", "-d")
        {
            Description = "milliseconds to delay",
            DefaultValueFactory = _ => 200
        };

        Option<int> simulOption = new("--simultaneous", "-s")
        {
            Description = "simultaneous connections when pulling",
            DefaultValueFactory = _ => 5
        };

        Option<string> username = new("--username", "-u")
        {
            Description = "Username for which to pull scrobbles"
        };

        Option<bool> dontAdd = new("--no-add", "-na")
        {
            Description = "Don't add any scrobbles to the database"
        };

        Option<bool> dontRemove = new("--no-remove", "-nr")
        {
            Description = "Don't remove any scrobbles from the database"
        };

        Command scrobbleSaveCmd = new("save", "save scrobbles to database");
        command.Subcommands.Add(scrobbleSaveCmd);


        scrobbleSaveCmd.SetAction((parseResult, cancellationToken) => ScrobbleSaveCommand.Execute(
            parseResult.GetValue(fromOption),
            parseResult.GetValue(toOption),
            parseResult.GetValue(pageOption),
            parseResult.GetValue(delayOption),
            parseResult.GetValue(simulOption),
            parseResult.GetValue(username),
            parseResult.GetValue(dontAdd),
            parseResult.GetValue(dontRemove),
            cancellationToken
        ));

        return command;
    }

    public static Command AddScrobbleClear(this Command command)
    {
        Option<DateTime?> fromOption = new("--from")
        {
            Description = "Date from which to pull scrobbles"
        };
        Option<DateTime?> toOption = new("--to")
        {
            Description = "Last date for which to pull scrobbles"
        };
        Option<string> username = new("--username", "-u")
        {
            Description = "Username for which to pull scrobbles"
        };
        Command scrobbleClearCmd = new("clear", "clear user scrobbles")
        {
            fromOption, toOption, username
        };
        command.Subcommands.Add(scrobbleClearCmd);
        scrobbleClearCmd.SetAction((parseResult, cancellationToken) => ScrobbleClearCommand.Execute(
            parseResult.GetValue(fromOption),
            parseResult.GetValue(toOption),
            parseResult.GetRequiredValue(username),
            cancellationToken
        ));

        return command;
    }

    public static Command AddScrobbleMap(this Command command)
    {
        Option<int> delayOption = new("--delay", "-d")
        {
            Description = "milliseconds to delay",
            DefaultValueFactory = _ => 100
        };
        Option<int> simulOption = new("--simultaneous", "-s")
        {
            Description = "simultaneous connections when pulling",
            DefaultValueFactory = _ => 3
        };
        Option<int?> limitOption = new("--limit", "-l")
        {
            Description = "limit number of objects to poll",
            DefaultValueFactory = _ => 200
        };

        Command scrobbleMapCmd = new("map", "map last.fm data to spotify uris")
        {
            delayOption, simulOption, limitOption
        };
        command.Subcommands.Add(scrobbleMapCmd);

        scrobbleMapCmd.SetAction((parseResult, cancellationToken) => ScrobbleMapCommand.Execute(
            parseResult.GetValue(delayOption),
            parseResult.GetValue(simulOption),
            parseResult.GetRequiredValue(limitOption),
            cancellationToken
        ));

        return command;
    }

    public static Command AddMigrateCommand(this Command command)
    {
        Option<string> connectionString = new("--connection", "-c")
        {
            Description = "database to migrate"
        };

        Command migrateCmd = new("migrate", "Migrate database")
        {
            connectionString
        };
        command.Subcommands.Add(migrateCmd);

        migrateCmd.SetAction((parseResult, cancellationToken) =>
            {
                MigrateCommand.Execute(parseResult.GetValue(connectionString));

                return Task.CompletedTask;
            }
        );

        return command;
    }

    public static Command AddSpotifyHistoryCommand(this Command command)
    {
        Option<string> connectionString = new("--connection", "-c")
        {
            Description = "database to migrate"
        };

        Option<string> pathString = new("--path", "-i")
        {
            Description = "path to find data"
        };

        Option<string> username = new("--username", "-u")
        {
            Description = "user to pulls scrobbles for"
        };

        Command historyCmd = new("history", "Insert Spotify history")
        {
            connectionString
        };
        command.Subcommands.Add(historyCmd);

        historyCmd.SetAction((parseResult, cancellationToken) =>
            {
                SpotifyHistoryCommand.Execute(
                    parseResult.GetValue(connectionString),
                    parseResult.GetValue(pathString),
                    parseResult.GetValue(username)
                );

                return Task.CompletedTask;
            }
        );

        return command;
    }
}