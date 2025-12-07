using System.CommandLine;
using Selector.CLI;
using Selector.CLI.Extensions;

RootCommand cmd = new();
cmd.SetAction((parseResult, cancellationToken) => HostCommand.Execute(cancellationToken));

cmd.AddScrobbleCommand();

#if !AOT
cmd.AddMigrateCommand();
#endif
cmd.AddSpotifyHistoryCommand();

var result = cmd.Parse(args);
result.Invoke();