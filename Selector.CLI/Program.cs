using System.CommandLine;

namespace Selector.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cmd = new HostRootCommand();
            cmd.AddCommand(new ScrobbleCommand("scrobble", "Manipulate scrobbles"));
            #if !AOT
            cmd.AddCommand(new MigrateCommand("migrate", "Migrate database"));
            #endif
            cmd.AddCommand(new SpotifyHistoryCommand("history", "Insert Spotify history"));

            cmd.Invoke(args);
        }
    }
}
