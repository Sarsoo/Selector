using System;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;

using Selector.Model;
using Selector.Cache;
using IF.Lastfm.Core.Api;
using StackExchange.Redis;

namespace Selector.CLI
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var cmd = new RootCommand {
                new Command("start") {

                }
            };

            var host = CreateHostBuilder(args, ConfigureDefault, ConfigureDefaultNlog);
            await host.RunConsoleAsync();
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

        public static void ConfigureLastFm(RootOptions config, IServiceCollection services)
        {
            if(config.LastfmClient is not null)
            {
                Console.WriteLine("> Adding Last.fm credentials...");

                var lastAuth = new LastAuth(config.LastfmClient, config.LastfmSecret);
                services.AddSingleton<LastAuth>(lastAuth);
            }
            else 
            {
                Console.WriteLine("> No Last.fm credentials, skipping init...");
            }
        }

        public static void ConfigureDb(RootOptions config, IServiceCollection services)
        {
            if (config.DatabaseOptions.Enabled)
            {
                Console.WriteLine("> Adding Databse Context...");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(config.DatabaseOptions.ConnectionString)
                );
            }
        }

        public static void ConfigureRedis(RootOptions config, IServiceCollection services)
        {
           if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Configuring Redis...");
                
                if(string.IsNullOrWhiteSpace(config.RedisOptions.ConnectionString))
                {
                    Console.WriteLine("> No Redis configuration string provided, exiting...");
                    Environment.Exit(1);
                }

                var connMulti = ConnectionMultiplexer.Connect(config.RedisOptions.ConnectionString);
                services.AddSingleton(connMulti);
                services.AddTransient<IDatabaseAsync>(services => services.GetService<ConnectionMultiplexer>().GetDatabase());
                services.AddTransient<ISubscriber>(services => services.GetService<ConnectionMultiplexer>().GetSubscriber());
            } 
        }

        public static void ConfigureEqual(RootOptions config, IServiceCollection services)
        {
            switch (config.Equality)
            {
                case EqualityChecker.Uri:
                    Console.WriteLine("> Using Uri Equality");
                    services.AddSingleton<IEqual, UriEqual>();
                    break;
                case EqualityChecker.String:
                    Console.WriteLine("> Using String Equality");
                    services.AddSingleton<IEqual, StringEqual>();
                    break;
            }
        }

        public static void ConfigureDefault(HostBuilderContext context, IServiceCollection services)
        {
            Console.WriteLine("~~~ Selector CLI ~~~");
            Console.WriteLine();

            Console.WriteLine("> Configuring...");
            // CONFIG
            var config = ConfigureOptions(context, services);

            Console.WriteLine("> Adding Services...");
            // SERVICES
            services.AddSingleton<IWatcherFactory, WatcherFactory>();
            services.AddSingleton<IAudioFeatureInjectorFactory, AudioFeatureInjectorFactory>();
            services.AddSingleton<IWatcherCollectionFactory, WatcherCollectionFactory>();
            // For generating spotify clients
            //services.AddSingleton<IRefreshTokenFactoryProvider, RefreshTokenFactoryProvider>();
            services.AddSingleton<IRefreshTokenFactoryProvider, CachingRefreshTokenFactoryProvider>();

            ConfigureLastFm(config, services);
            ConfigureDb(config, services);
            ConfigureRedis(config, services);
            ConfigureEqual(config, services);

            // HOSTED SERVICES
            if (config.WatcherOptions.Enabled)
            {
                Console.WriteLine("> Adding Watcher Service");
                services.AddHostedService<WatcherService>();
            }
        }

        public static void ConfigureDefaultNlog(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog(context.Configuration);
        }

        static IHostBuilder CreateHostBuilder(string[] args, Action<HostBuilderContext, IServiceCollection> BuildServices, Action<HostBuilderContext, ILoggingBuilder> BuildLogs)
            => Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => BuildServices(context, services))
                .ConfigureLogging((context, builder) => BuildLogs(context, builder));
    }
}
