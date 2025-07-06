using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Selector.AppleMusic.Extensions;
using Selector.Cache.Extensions;
using Selector.CLI.Extensions;
using Selector.Events;
using Selector.Extensions;
using Selector.Model.Extensions;
using Selector.Spotify;

namespace Selector.CLI
{
    public class HostRootCommand : RootCommand
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
                var host = CreateHostBuilder(Environment.GetCommandLineArgs(), ConfigureDefault, ConfigureDefaultNlog)
                    .Build();

                var logger = host.Services.GetRequiredService<ILogger<HostCommand>>();
                var env = host.Services.GetRequiredService<IHostEnvironment>();
                SetupExceptionHandling(logger, env);

                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }

            return 0;
        }

        private static void SetupExceptionHandling(ILogger logger, IHostEnvironment env)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception thrown");

                    if (env.IsDevelopment())
                    {
                        throw ex;
                    }
                }
            };
        }

        public static RootOptions ConfigureOptions(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<RootOptions>(options =>
            {
                OptionsHelper.ConfigureOptions(options, context.Configuration);
            });

            var config = context.Configuration.ConfigureOptions();

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
            context.Configuration.ConfigureOptionsInjection(services);

            Console.WriteLine("> Adding Services...");
            // SERVICES
            services.AddHttpClient()
                .ConfigureDb(config);

            services.AddConsumerFactories();
            services.AddCLIConsumerFactories();
            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding caching consumers...");
                services.AddCachingConsumerFactories();
            }

            var tracing = context.Configuration.GetSection(TracingOptions.Key).Get<TracingOptions>();
            if (!string.IsNullOrWhiteSpace(tracing.Endpoint))
            {
                Console.WriteLine("> Adding OTel Tracing...");
                services.AddOpenTelemetry()
                    .ConfigureResource(resource => resource.AddService(tracing.ServiceName))
                    .WithTracing(b =>
                    {
                        b.AddHttpClientInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation()
                            .AddRedisInstrumentation()
                            .AddQuartzInstrumentation()
                            .AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(tracing.Endpoint);
                                options.Protocol = OtlpExportProtocol.HttpProtobuf;
                                options.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>()
                                {
                                    MaxExportBatchSize = 256,
                                    ScheduledDelayMilliseconds = 2500
                                };
                            });

                        foreach (var source in tracing.Sources)
                        {
                            b.AddSource(source);
                        }
                    });

                ActivitySource.AddActivityListener(new ActivityListener
                {
                    ShouldListenTo = _ => true,
                    ActivityStopped = activity =>
                    {
                        foreach (var (key, value) in activity.Baggage)
                        {
                            activity.AddTag(key, value);
                        }
                    }
                });
            }

            services.AddWatcher()
                .AddSpotifyWatcher()
                .AddEvents()
                .AddSpotify()
                .AddAppleMusic();

            services.ConfigureLastFm(config)
                .ConfigureEqual(config)
                .ConfigureJobs(config);

            if (config.RedisOptions.Enabled)
            {
                Console.WriteLine("> Adding Redis...");
                services.AddRedisServices(config.RedisOptions.ConnectionString);

                Console.WriteLine("> Adding cache event maps...");
                services.AddTransient<IEventMapping, FromPubSub.SpotifyLink>()
                    .AddTransient<IEventMapping, FromPubSub.AppleMusicLink>()
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
        }

        public static void ConfigureDefaultNlog(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders()
                .SetMinimumLevel(LogLevel.Trace)
                .AddNLog(context.Configuration);
        }

        static IHostBuilder CreateHostBuilder(string[] args,
            Action<HostBuilderContext, IServiceCollection> buildServices,
            Action<HostBuilderContext, ILoggingBuilder> buildLogs)
            => Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((context, services) => buildServices(context, services))
                .ConfigureLogging((context, builder) => buildLogs(context, builder));
    }
}