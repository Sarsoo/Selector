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
    public class ScrobbleClearCommand : Command
    {
        public ScrobbleClearCommand(string name, string description = null) : base(name, description) 
        {
            var fromOption = new Option<DateTime>("--from", "Date from which to pull scrobbles");
            AddOption(fromOption);

            var toOption = new Option<DateTime>( "--to", "Last date for which to pull scrobbles");
            AddOption(toOption);

            var username = new Option<string>("--username", "user to pulls scrobbles for");
            username.AddAlias("-u");
            AddOption(username);

            Handler = CommandHandler.Create(async (DateTime? from, DateTime? to, string username, CancellationToken token) => await Execute(from, to, username, token));
        }

        public static async Task<int> Execute(DateTime? from, DateTime? to, string username, CancellationToken token)
        {
            try
            {
                var context = new CommandContext().WithLogger().WithDb();
                var logger = context.Logger.CreateLogger("Scrobble");

                var db = new ApplicationDbContext(context.DatabaseConfig.Options, context.Logger.CreateLogger<ApplicationDbContext>());

                logger.LogInformation("Searching for {0}", username);
                var user = db.Users
                    .Include(u => u.Scrobbles)
                    .FirstOrDefault(u => u.UserName == username);

                if (user is not null)
                {
                    user.Scrobbles = user.Scrobbles
                        .Where(s => s.Timestamp < (from ?? DateTime.MinValue) 
                                    && s.Timestamp > (to ?? DateTime.MaxValue))
                        .ToList();

                    await db.SaveChangesAsync();
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
