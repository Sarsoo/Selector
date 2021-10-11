using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;

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
                .ConfigureServices((context, services) => {

                    // CONFIG
                    services.Configure<RootOptions>(options => {
                        context.Configuration.GetSection(RootOptions.Key).Bind(options);
                        context.Configuration.GetSection($"{RootOptions.Key}:{WatcherOptions.Key}").Bind(options.WatcherOptions);
                    });

                    // SERVICES
                    //services.AddTransient<IWatcherFactory, PlayerWatcher>();
                    //services.AddTransient<IWatcherCollection, WatcherCollection>();

                    switch(context.Configuration.GetValue<EqualityChecker>("selector:equality"))
                    {
                        case EqualityChecker.Uri:
                            services.AddTransient<IEqual, UriEqual>();
                            break;
                        case EqualityChecker.String:
                            services.AddTransient<IEqual, StringEqual>();
                            break;
                    }

                    // HOSTED SERVICES
                    services.AddHostedService<WatcherService>();
                })
                .ConfigureLogging((context, builder) => {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(context.Configuration);
                });
    }
}
