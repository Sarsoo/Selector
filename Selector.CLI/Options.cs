using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Selector.CLI
{
    static class OptionsHelper {
        public static void ConfigureOptions(RootOptions options, IConfiguration config)
        {
            config.GetSection(RootOptions.Key).Bind(options);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, WatcherOptions.Key})).Bind(options.WatcherOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, DatabaseOptions.Key})).Bind(options.DatabaseOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, RedisOptions.Key})).Bind(options.RedisOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, JobsOptions.Key})).Bind(options.JobOptions);
            config.GetSection(FormatKeys( new[] { RootOptions.Key, JobsOptions.Key, ScrobbleWatcherJobOptions.Key })).Bind(options.JobOptions.Scrobble);
        }  

        public static RootOptions ConfigureOptions(this IConfiguration config)
        {
            var options = config.GetSection(RootOptions.Key).Get<RootOptions>();

            ConfigureOptions(options, config);

            return options;
        }

        public static RootOptions ConfigureOptionsInjection(this IConfiguration config, IServiceCollection services)
        {
            var options = config.GetSection(RootOptions.Key).Get<RootOptions>();

            services.Configure<DatabaseOptions>(config.GetSection(FormatKeys(new[] { RootOptions.Key, DatabaseOptions.Key })));
            services.Configure<RedisOptions>(config.GetSection(FormatKeys(new[] { RootOptions.Key, RedisOptions.Key })));
            services.Configure<WatcherOptions>(config.GetSection(FormatKeys(new[] { RootOptions.Key, WatcherOptions.Key })));

            services.Configure<JobsOptions>(config.GetSection(FormatKeys(new[] { RootOptions.Key, JobsOptions.Key })));
            services.Configure<ScrobbleWatcherJobOptions>(config.GetSection(FormatKeys(new[] { RootOptions.Key, JobsOptions.Key, ScrobbleWatcherJobOptions.Key })));

            return options;
        }

        public static string FormatKeys(string[] args) => string.Join(":", args);
    }

    public class RootOptions
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
        /// <summary>
        /// Service account refresh token for tool spotify usage
        /// </summary>
        public string RefreshToken { get; set; }
        public string LastfmClient { get; set; }
        public string LastfmSecret { get; set; }
        public WatcherOptions WatcherOptions { get; set; } = new();
        public JobsOptions JobOptions { get; set; } = new();
        public DatabaseOptions DatabaseOptions { get; set; } = new();
        public RedisOptions RedisOptions { get; set; } = new();
        public EqualityChecker Equality { get; set; } = EqualityChecker.Uri;
    }

    public enum EqualityChecker
    {
        Uri, String
    }

    public class WatcherOptions
    {
        public const string Key = "Watcher";

        public bool Enabled { get; set; } = true;
        public bool LocalEnabled { get; set; } = true;
        public List<WatcherInstanceOptions> Instances { get; set; } = new();
    }

    public class WatcherInstanceOptions
    {
        public const string Key = "Instances";

        public string Name { get; set; }
        public string AccessKey { get; set; }
        public string RefreshKey { get; set; }
        public string LastFmUsername { get; set; }
        public int PollPeriod { get; set; } = 5000;
        public WatcherType Type { get; set; } = WatcherType.Player;
        public List<Consumers> Consumers { get; set; } = default;
#nullable enable
        public string? PlaylistUri { get; set; }
        public string? WatcherCollection { get; set; }
#nullable disable
    }

    public enum Consumers
    {
        AudioFeatures, AudioFeaturesCache, CacheWriter, Publisher, PlayCounter, MappingPersister
    }

    public class RedisOptions
    {
        public const string Key = "Redis";

        public bool Enabled { get; set; } = false;
        public string ConnectionString { get; set; }
    }

    public class JobsOptions
    {
        public const string Key = "Job";

        public bool Enabled { get; set; } = true;
        public ScrobbleWatcherJobOptions Scrobble { get; set; } = new();
    }

    public class ScrobbleWatcherJobOptions
    {
        public const string Key = "Scrobble";

        public bool Enabled { get; set; } = true;
        public string FullScrobbleCron { get; set; } = "0 0 2 * * ?";
        public TimeSpan InterJobDelay { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan InterRequestDelay { get; set; } = TimeSpan.FromMilliseconds(100);
        public DateTime? From { get; set; } = DateTime.UtcNow.AddDays(-14);
        public int PageSize { get; set; } = 200;
        public int Simultaneous { get; set; } = 3;
    }
}
