using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Selector.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                //.AddLogging(b =>
                //    b.AddFilter("Microsoft", LogLevel.Warning)
                //        .AddFilter("System", LogLevel.Warning)
                //        .AddFilter("Selector.CLI.Program", LogLevel.Debug)
                //        .AddConsole()
                //)
                .AddTransient<IPlayerWatcher, PlayerWatcher>()
                .AddTransient<IWatcherCollection, WatcherCollection>()
                .BuildServiceProvider();

            //using var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddFilter("Microsoft", LogLevel.Warning)
            //        .AddFilter("System", LogLevel.Warning)
            //        .AddFilter("Selector.CLI.Program", LogLevel.Debug)
            //        .AddConsole();
            //});
            //ILogger logger = loggerFactory.CreateLogger<Program>();
            //logger.LogInformation("Example log message");

            //var logger = serviceProvider.GetService<ILoggerFactory>()
            //    .CreateLogger<Program>();
            //logger.LogDebug("Starting application");

            var logger = serviceProvider.GetService<ILogger>();

            logger.LogDebug("All done!");
        }
    }
}
