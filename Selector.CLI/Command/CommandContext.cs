using IF.Lastfm.Core.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.Model;

namespace Selector.CLI
{
    public class CommandContext
    {
        public RootOptions Config { get; set; }
        public ILoggerFactory Logger { get; set; }

        public DbContextOptionsBuilder<ApplicationDbContext> DatabaseConfig { get; set; }        
        public LastfmClient LastFmClient { get; set; }
    }
}
