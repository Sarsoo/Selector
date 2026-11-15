using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.CLI
{
    public class ScrobbleMapCommand : Command
    {
        public ScrobbleMapCommand(string name, string description = null) : base(name, description)
        {
            var delayOption = new Option<int>("--delay")
            {
                DefaultValueFactory = (_) => 100,
                Description = "milliseconds to delay"
            };
            delayOption.Aliases.Add("-d");
            Add(delayOption);

            var simulOption = new Option<int>("--simultaneous")
            {
                DefaultValueFactory = (_) => 3,
                Description = "simultaneous connections when pulling"
            };
            simulOption.Aliases.Add("-s");
            Add(simulOption);

            var limitOption = new Option<int?>("--limit")
            {
                DefaultValueFactory = (_) => 200,
                Description = "limit number of objects to poll"
            };
            limitOption.Aliases.Add("-l");
            Add(limitOption);

            SetAction(async (pr, cr) =>
            {
                await Execute(
                    pr.GetValue(delayOption),
                    pr.GetValue(simulOption),
                    pr.GetValue(limitOption),
                    cr
                );
            });
        }

        public static async Task<int> Execute(int delay, int simultaneous, int? limit, CancellationToken token)
        {
            try
            {
                var context = new CommandContext().WithLogger().WithDb().WithSpotify();
                var logger = context.Logger.CreateLogger("Scrobble");

                using var db = new ApplicationDbContext(context.DatabaseConfig.Options, context.Logger.CreateLogger<ApplicationDbContext>());

                await new ScrobbleMapper(
                    context.Spotify.Search,
                    new ()
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
}
