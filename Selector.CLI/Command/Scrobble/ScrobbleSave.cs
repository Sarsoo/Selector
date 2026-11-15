using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.CLI.Extensions;
using Selector.Model;
using Selector.Model.Extensions;
using System;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Selector.CLI
{
    public class ScrobbleSaveCommand : Command
    {
        public ScrobbleSaveCommand(string name, string description = null) : base(name, description) 
        {
            var fromOption = new Option<DateTime>("--from")
            {
                DefaultValueFactory = (_) => DateTime.UtcNow.AddDays(-7),
                Description = "Date from which to pull scrobbles"
            };
            Add(fromOption);

            var toOption = new Option<DateTime>( "--to")
            {
                DefaultValueFactory = (_) => DateTime.UtcNow.AddHours(1),
                Description = "Last date for which to pull scrobbles"
            };
            Add(toOption);

            var pageOption = new Option<int>("--page")
            {
                DefaultValueFactory = (_) => 100,
                Description = "number of scrobbles per page"
            };
            pageOption.Aliases.Add("-p");
            Add(pageOption);

            var delayOption = new Option<int>("--delay")
            {
                DefaultValueFactory = (_) => 200,
                Description = "milliseconds to delay"
            };
            delayOption.Aliases.Add("-d");
            Add(delayOption);

            var simulOption = new Option<int>("--simultaneous")
            {
                DefaultValueFactory = (_) => 5,
                Description = "simultaneous connections when pulling"
            };
            simulOption.Aliases.Add("-s");
            Add(simulOption);

            var username = new Option<string>("--username")
            {
                Description = "user to pulls scrobbles for"
            };
            username.Aliases.Add("-u");
            Add(username);

            var dontAdd = new Option<bool>("--no-add")
            {
                Description = "don't add any scrobbles to the database"
            };
            dontAdd.Aliases.Add("-na");
            Add(dontAdd);

            var dontRemove = new Option<bool>("--no-remove")
            {
                Description = "don't remove any scrobbles from the database"
            };
            dontRemove.Aliases.Add("-nr");
            Add(dontRemove);

            SetAction(async (pr, cr) =>
            {
                await Execute(
                    pr.GetValue(fromOption),
                    pr.GetValue(toOption),
                    pr.GetValue(pageOption),
                    pr.GetValue(delayOption),
                    pr.GetValue(simulOption),
                    pr.GetValue(username),
                    pr.GetValue(dontAdd),
                    pr.GetValue(dontRemove),
                    cr
                );
            });
        }

        public static async Task<int> Execute(DateTime from, DateTime to, int page, int delay, int simultaneous, string username, bool noAdd, bool noRemove, CancellationToken token)
        {
            try
            {
                var context = new CommandContext().WithLogger().WithDb().WithLastfmApi();
                var logger = context.Logger.CreateLogger("Scrobble");

                using var db = new ApplicationDbContext(context.DatabaseConfig.Options, context.Logger.CreateLogger<ApplicationDbContext>());
                var repo = new ScrobbleRepository(db);

                logger.LogInformation("Running from {} to {}", from, to);

                logger.LogInformation("Searching for {}", username);
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.UserName == username);

                if (user is not null)
                {
                    if (user.LastFmConnected())
                    {
                        logger.LogInformation("Last.fm username found ({}), starting...", user.LastFmUsername);

                        if(from.Kind != DateTimeKind.Utc)
                        {
                            from = from.ToUniversalTime();
                        }

                        if (to.Kind != DateTimeKind.Utc)
                        {
                            to = to.ToUniversalTime();
                        }

                        await new ScrobbleSaver(
                            context.LastFmClient.User,
                            new ()
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
}
