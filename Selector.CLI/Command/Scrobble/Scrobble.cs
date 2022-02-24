using System.CommandLine;

namespace Selector.CLI
{
    public class ScrobbleCommand : Command
    {
        public ScrobbleCommand(string name, string description = null) : base(name, description)
        {
            var saveCommand = new ScrobbleSaveCommand("save", "save scrobbles to database");
            AddCommand(saveCommand);

            var clearCommand = new ScrobbleClearCommand("clear", "clear user scrobbles");
            AddCommand(clearCommand);

            var mapCommand = new ScrobbleMapCommand("map", "map last.fm data to spotify uris");
            AddCommand(mapCommand);
        }
    }
}
