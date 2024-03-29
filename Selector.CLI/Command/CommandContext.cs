﻿using IF.Lastfm.Core.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Selector.Model;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.CLI
{
    public class CommandContext
    {
        public RootOptions Config { get; set; }
        public ILoggerFactory Logger { get; set; }
        public ISpotifyClient Spotify { get; set; }
        public ConnectionMultiplexer RedisMux { get; set; }

        public DbContextOptionsBuilder<ApplicationDbContext> DatabaseConfig { get; set; }        
        public LastfmClient LastFmClient { get; set; }
    }
}
