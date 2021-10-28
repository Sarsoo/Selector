using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Selector.CLI
{
    static class OptionsHelper {
        public static void ConfigureOptions(RootOptions options, IConfiguration config)
        {
            config.GetSection(RootOptions.Key).Bind(options);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, WatcherOptions.Key})).Bind(options.WatcherOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, DatabaseOptions.Key})).Bind(options.DatabaseOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, RedisOptions.Key})).Bind(options.RedisOptions);
        }  

        public static RootOptions ConfigureOptions(IConfiguration config)
        {
            var options = config.GetSection(RootOptions.Key).Get<RootOptions>();
            ConfigureOptions(options, config);
            return options;
        }  

        public static string FormatKeys(string[] args) => string.Join(":", args);
    }

    class RootOptions
    {
        public const string Key = "Selector";

        /// <summary>
        /// Spotify client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Spotify app secret
        /// </summary>
        public string ClientSecret { get; set; }
        public WatcherOptions WatcherOptions { get; set; } = new();
        public DatabaseOptions DatabaseOptions { get; set; } = new();
        public RedisOptions RedisOptions { get; set; } = new();
        public EqualityChecker Equality { get; set; } = EqualityChecker.Uri;
    }

    enum EqualityChecker
    {
        Uri, String
    }

    class WatcherOptions
    {
        public const string Key = "Watcher";

        public bool Enabled { get; set; } = true;
        public List<WatcherInstanceOptions> Instances { get; set; } = new();
    }

    class WatcherInstanceOptions
    {
        public const string Key = "Instances";

        public string Name { get; set; }
        public string AccessKey { get; set; }
        public string RefreshKey { get; set; }
        public int PollPeriod { get; set; } = 5000;
        public WatcherType Type { get; set; } = WatcherType.Player;
        public List<Consumers> Consumers { get; set; } = default;
#nullable enable
        public string? PlaylistUri { get; set; }
        public string? WatcherCollection { get; set; }
#nullable disable
    }

    enum Consumers
    {
        AudioFeatures, CacheWriter, Publisher
    }

    class DatabaseOptions {
        public const string Key = "Database";

        public bool Enabled { get; set; } = false;
        public string ConnectionString { get; set; }
    }

    class RedisOptions
    {
        public const string Key = "Redis";

        public bool Enabled { get; set; } = false;
        public string ConnectionString { get; set; }
    }
}
