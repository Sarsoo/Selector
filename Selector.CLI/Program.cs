using System.CommandLine;

namespace Selector.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cmd = new HostRootCommand();
            cmd.AddCommand(new ScrobbleCommand("scrobble", "Manipulate scrobbles"));

            cmd.Invoke(args);
        }
    }
}
