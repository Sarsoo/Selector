using System.Threading.Tasks;

namespace Selector.CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var cmd = new HostRootCommand();
        cmd.Add(new ScrobbleCommand("scrobble", "Manipulate scrobbles"));
#if !AOT
        cmd.Add(new MigrateCommand("migrate", "Migrate database"));
#endif
        cmd.Add(new SpotifyHistoryCommand("history", "Insert Spotify history"));

        return await cmd.Parse(args).InvokeAsync();
    }
}