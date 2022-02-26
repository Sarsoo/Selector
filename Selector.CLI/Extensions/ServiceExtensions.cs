using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Selector.Cache.Extensions;
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

                    var scrobbleKey = new JobKey("scrobble-watcher", "scrobble");

                    options.AddJob<ScrobbleWatcherJob>(j => j
                        .WithDescription("Watch recent scrobbles and mirror to database")
                        .WithIdentity(scrobbleKey)
                    );

                    options.AddTrigger(t => t
                        .WithIdentity("scrobble-watcher-trigger")
                        .ForJob(scrobbleKey)
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithInterval(config.JobOptions.Scrobble.InterJobDelay).RepeatForever())
                        .WithDescription("Periodic trigger for scrobble watcher")
                    );
                });

                services.AddQuartzHostedService(options =>{

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

                services.AddTransient<IScrobbleRepository, ScrobbleRepository>();
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
    }    
}
