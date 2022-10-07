using System.CommandLine;

namespace Selector.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cmd = new HostRootCommand();
            cmd.AddCommand(new ScrobbleCommand("scrobble", "Manipulate scrobbles"));
            cmd.AddCommand(new MigrateCommand("migrate", "Migrate database"));
            cmd.AddCommand(new SpotifyHistoryCommand("history", "Insert Spotify history"));

            cmd.Invoke(args);
        }
    }
}
