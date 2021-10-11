using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
                    services.AddTransient<IPlayerWatcher, PlayerWatcher>();
                    services.AddTransient<IWatcherCollection, WatcherCollection>();
                    services.AddHostedService<WatcherService>();
                })
                .ConfigureLogging(builder => {
                    builder
                        .AddSimpleConsole(options => {
                            options.IncludeScopes = true;
                            options.SingleLine = true;
                            options.TimestampFormat = "yyyy-mm-dd hh:mm:ss ";
                        });
                });
    }
}
