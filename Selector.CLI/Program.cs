using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;

using Selector.Model;

namespace Selector.CLI
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    Console.WriteLine("~~~ Selector CLI ~~~");
                    Console.WriteLine();

                    Console.WriteLine("> Configuring...");
                    // CONFIG
                    services.Configure<RootOptions>(options =>
                    {
                        OptionsHelper.ConfigureOptions(options, context.Configuration);
                    });
                    var config = OptionsHelper.ConfigureOptions(context.Configuration);

                    Console.WriteLine("> Adding Services...");
                    // SERVICES
                    services.AddSingleton<IWatcherFactory, WatcherFactory>();
                    services.AddSingleton<IConsumerFactory, AudioFeatureInjectorFactory>();
                    services.AddSingleton<IWatcherCollectionFactory, WatcherCollectionFactory>();
                    // For generating spotify clients
                    //services.AddSingleton<IRefreshTokenFactoryProvider, RefreshTokenFactoryProvider>();
                    services.AddSingleton<IRefreshTokenFactoryProvider, CachingRefreshTokenFactoryProvider>();

                    if (config.DatabaseOptions.Enabled)
                    {
                        Console.WriteLine("> Adding Databse Context...");
                        services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseNpgsql(config.DatabaseOptions.ConnectionString)
                        );
                    }

                    switch (config.Equality)
                    {
                        case EqualityChecker.Uri:
                            Console.WriteLine("> Using Uri Equality");
                            services.AddTransient<IEqual, UriEqual>();
                            break;
                        case EqualityChecker.String:
                            Console.WriteLine("> Using String Equality");
                            services.AddTransient<IEqual, StringEqual>();
                            break;
                    }

                    // HOSTED SERVICES
                    if (config.WatcherOptions.Enabled)
                    {
                        Console.WriteLine("> Adding Watcher Service");
                        services.AddHostedService<WatcherService>();
                    }

                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(context.Configuration);
                });
    }
}
