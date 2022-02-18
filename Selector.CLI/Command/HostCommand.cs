using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Selector.Cache.Extensions;
using Selector.CLI.Extensions;
using Selector.CLI.Services;
using Selector.Events;
using Selector.Extensions;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Selector.CLI
{
    public class HostRootCommand: RootCommand
    {
        public HostRootCommand()
        {
            Handler = CommandHandler.Create(() => HostCommand.Execute());
        }
    }

    public class HostCommand : Command
    {
        public HostCommand(string name, string description = null) : base(name, description)
        {
            Handler = CommandHandler.Create(() => Execute());
        }

        public static int Execute()
        {
            try
            {
                CreateHostBuilder(Environment.GetCommandLineArgs(), ConfigureDefault, ConfigureDefaultNlog)
                    .Build()
                    .Run();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        public static RootOptions ConfigureOptions(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<RootOptions>(options =>
            {
                OptionsHelper.ConfigureOptions(options, context.Configuration);
            });

            var config = OptionsHelper.ConfigureOptions(context.Configuration);

            services.Configure<SpotifyAppCredentials>(options =>
            {
                options.ClientId = config.ClientId;
                options.ClientSecret = config.ClientSecret;
            });

            return config;
        }

        public static void ConfigureDefault(HostBuilderContext context, IServiceCollection services)
        {
            Console.WriteLine("~~~ Selector CLI ~~~");
            Console.WriteLine();

            Console.WriteLine("> Configuring...");
            // CONFIG
            var config = ConfigureOptions(context, services);
            services.AddHttpClient();

            Console.WriteLine("> Adding Services...");
            // SERVICES
            services.AddConsumerFactories();
            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding caching consumers...");
                services.AddCachingConsumerFactories();
            }

            services.AddWatcher()
                    .AddEvents()
                    .AddSpotify();

            services.ConfigureLastFm(config)
                    .ConfigureDb(config)
                    .ConfigureEqual(config);

            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding Redis...");
                services.AddRedisServices(config.RedisOptions.ConnectionString);

                Console.WriteLine("> Adding cache event maps...");
                services.AddTransient<IEventMapping, FromPubSub.SpotifyLink>()
                        .AddTransient<IEventMapping, FromPubSub.Lastfm>();

                Console.WriteLine("> Adding caching Spotify consumers...");
                services.AddCachingSpotify();
            }

            // HOSTED SERVICES
            if (config.WatcherOptions.Enabled)
            {
                if (config.WatcherOptions.LocalEnabled)
                {
                    Console.WriteLine("> Adding Local Watcher Service");
                    services.AddHostedService<LocalWatcherService>();
                }

                if (config.DatabaseOptions.Enabled)
                {
                    Console.WriteLine("> Adding Db Watcher Service");
                    services.AddHostedService<DbWatcherService>();
                }
            }

            if (config.ScrobbleOptions.Enabled)
            {
                Console.WriteLine("> Adding Scrobble Monitor Service");

                services.AddHostedService<ScrobbleMonitor>();
            }
        }

        public static void ConfigureDefaultNlog(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders()
                   .SetMinimumLevel(LogLevel.Trace)
                   .AddNLog(context.Configuration);
        }

        static IHostBuilder CreateHostBuilder(string[] args, Action<HostBuilderContext, IServiceCollection> BuildServices, Action<HostBuilderContext, ILoggingBuilder> BuildLogs)
            => Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureServices((context, services) => BuildServices(context, services))
                .ConfigureLogging((context, builder) => BuildLogs(context, builder));
    }
}
