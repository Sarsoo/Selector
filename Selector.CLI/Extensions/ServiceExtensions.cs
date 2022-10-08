using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Selector.Cache.Extensions;
using Selector.CLI.Consumer;
using Selector.CLI.Jobs;
using Selector.Extensions;
using Selector.Model;
using Selector.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.CLI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection ConfigureLastFm(this IServiceCollection services, RootOptions config)
        {
            if (config.LastfmClient is not null)
            {
                Console.WriteLine("> Adding Last.fm credentials...");

                services.AddLastFm(config.LastfmClient, config.LastfmSecret);

                if (config.RedisOptions.Enabled)
                {
                    Console.WriteLine("> Adding caching Last.fm consumers...");
                    services.AddCachingLastFm();
                }
            }
            else
            {
                Console.WriteLine("> No Last.fm credentials, skipping init...");
            }

            return services;
        }

        public static IServiceCollection ConfigureJobs(this IServiceCollection services, RootOptions config)
        {
            if (config.JobOptions.Enabled)
            {
                Console.WriteLine("> Adding Jobs...");

                services.AddQuartz(options => {

                    options.UseMicrosoftDependencyInjectionJobFactory();

                    options.UseSimpleTypeLoader();
                    options.UseInMemoryStore();
                    options.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 5;
                    });

                    if (config.JobOptions.Scrobble.Enabled)
                    {
                        Console.WriteLine("> Adding Scrobble Jobs...");

                        var scrobbleKey = new JobKey("scrobble-watcher-agile", "scrobble");

                        options.AddJob<ScrobbleWatcherJob>(j => j
                            .WithDescription("Watch recent scrobbles and mirror to database")
                            .WithIdentity(scrobbleKey)
                            .UsingJobData("IsFull", false)
                        );

                        options.AddTrigger(t => t
                            .WithIdentity("scrobble-watcher-agile-trigger")
                            .ForJob(scrobbleKey)
                            .StartNow()
                            .WithSimpleSchedule(x => x.WithInterval(config.JobOptions.Scrobble.InterJobDelay).RepeatForever())
                            .WithDescription("Periodic trigger for scrobble watcher")
                        );

                        var fullScrobbleKey = new JobKey("scrobble-watcher-full", "scrobble");

                        options.AddJob<ScrobbleWatcherJob>(j => j
                            .WithDescription("Check all scrobbles and mirror to database")
                            .WithIdentity(fullScrobbleKey)
                            .UsingJobData("IsFull", true)
                        );

                        options.AddTrigger(t => t
                            .WithIdentity("scrobble-watcher-full-trigger")
                            .ForJob(fullScrobbleKey)
                            .WithCronSchedule(config.JobOptions.Scrobble.FullScrobbleCron)
                            .WithDescription("Periodic trigger for scrobble watcher")
                        );
                    }   
                    else
                    {
                        Console.WriteLine("> Skipping Scrobble Jobs...");
                    }
                });

                services.AddQuartzHostedService(options => {

                    options.WaitForJobsToComplete = true;
                });

                services.AddTransient<ScrobbleWatcherJob>();
                services.AddTransient<IJob, ScrobbleWatcherJob>();
            }

            return services;
        }

        public static IServiceCollection ConfigureDb(this IServiceCollection services, RootOptions config)
        {
            if (config.DatabaseOptions.Enabled)
            {
                Console.WriteLine("> Adding Databse Context...");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(config.DatabaseOptions.ConnectionString)
                );

                services.AddTransient<IScrobbleRepository, ScrobbleRepository>()
                        .AddTransient<ISpotifyListenRepository, SpotifyListenRepository>();

                services.AddTransient<IListenRepository, MetaListenRepository>();
                //services.AddTransient<IListenRepository, SpotifyListenRepository>();

                services.AddTransient<IScrobbleMappingRepository, ScrobbleMappingRepository>();

                services.AddHostedService<MigratorService>();
            }

            return services;
        }

        public static IServiceCollection ConfigureEqual(this IServiceCollection services, RootOptions config)
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

            return services;
        }

        public static IServiceCollection AddCLIConsumerFactories(this IServiceCollection services)
        {
            services.AddTransient<IMappingPersisterFactory, MappingPersisterFactory>();
            services.AddTransient<MappingPersisterFactory>();

            return services;
        }
    }    
}
