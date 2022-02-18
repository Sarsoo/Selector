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
    public class ScrobbleCommand : Command
    {
        public ScrobbleCommand(string name, string description = null) : base(name, description)
        {

            var saveCommand = new ScrobbleSaveCommand("save", "save scrobbles to");
            AddCommand(saveCommand);
        }
    }

    public class ScrobbleSaveCommand : Command
    {
        public ScrobbleSaveCommand(string name, string description = null) : base(name, description) 
        {
            var fromOption = new Option<DateTime>("--from", getDefaultValue: () => DateTime.UtcNow.AddDays(-7), "Date from which to pull scrobbles");
            AddOption(fromOption);

            var toOption = new Option<DateTime>( "--to", getDefaultValue: () => DateTime.UtcNow.AddHours(1), "Last date for which to pull scrobbles");
            AddOption(toOption);

            var pageOption = new Option<int>("--page", getDefaultValue: () => 100, "number of scrobbles per page");
            pageOption.AddAlias("-p");
            AddOption(pageOption);

            var delayOption = new Option<int>("--delay", getDefaultValue: () => 200, "milliseconds to delay");
            delayOption.AddAlias("-d");
            AddOption(delayOption);

            var username = new Option<string>("--username", "user to pulls scrobbles for");
            username.AddAlias("-u");
            AddOption(username);

            var dontAdd = new Option("--no-add", "don't add any scrobbles to the database");
            dontAdd.AddAlias("-na");
            AddOption(dontAdd);

            var dontRemove = new Option("--no-remove", "don't remove any scrobbles from the database");
            dontRemove.AddAlias("-nr");
            AddOption(dontRemove);

            Handler = CommandHandler.Create(async (DateTime from, DateTime to, int page, int delay, string username, bool noAdd, bool noRemove, CancellationToken token) => await Execute(from, to, page, delay, username, noAdd, noRemove, token));
        }

        public static async Task<int> Execute(DateTime from, DateTime to, int page, int delay, string username, bool noAdd, bool noRemove, CancellationToken token)
        {
            try
            {
                var context = new CommandContext().WithLogger().WithDb().WithLastfmApi();
                var logger = context.Logger.CreateLogger("Scrobble");

                var db = new ApplicationDbContext(context.DatabaseConfig.Options, context.Logger.CreateLogger<ApplicationDbContext>());

                logger.LogInformation("Running from {0} to {1}", from, to);

                logger.LogInformation("Searching for {0}", username);
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.UserName == username);

                if (user is not null)
                {
                    if (user.LastFmConnected())
                    {
                        logger.LogInformation("Last.fm username found ({0}), starting...", user.LastFmUsername);

                        await new ScrobbleSaver(
                            context.LastFmClient.User,
                            new ScrobbleSaverConfig()
                            {
                                User = user,
                                InterRequestDelay = new TimeSpan(0, 0, 0, 0, delay),
                                From = from,
                                To = to,
                                PageSize = page,
                                DontAdd = noAdd,
                                DontRemove = noRemove
                            },
                            db,
                            context.Logger.CreateLogger<ScrobbleSaver>())
                                .Execute(token);
                    }
                    else
                    {
                        logger.LogError("{0} doesn't have a Last.fm username", username);
                    }
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
}
