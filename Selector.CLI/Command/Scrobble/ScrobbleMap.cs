using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;
using Selector.Model.Extensions;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.CLI
{
    public class ScrobbleMapCommand : Command
    {
        public ScrobbleMapCommand(string name, string description = null) : base(name, description)
        {
            var delayOption = new Option<int>("--delay", getDefaultValue: () => 100, "milliseconds to delay");
            delayOption.AddAlias("-d");
            AddOption(delayOption);

            var simulOption = new Option<int>("--simultaneous", getDefaultValue: () => 3, "simultaneous connections when pulling");
            simulOption.AddAlias("-s");
            AddOption(simulOption);

            var limitOption = new Option<int?>("--limit", getDefaultValue: () => 200, "limit number of objects to poll");
            limitOption.AddAlias("-l");
            AddOption(limitOption);

            Handler = CommandHandler.Create(async (int delay, int simultaneous, int? limit, CancellationToken token) => await Execute(delay, simultaneous, limit, token));
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
