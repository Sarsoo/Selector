using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Selector.Cache.Extensions;
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

        public static IServiceCollection ConfigureDb(this IServiceCollection services, RootOptions config)
        {
            if (config.DatabaseOptions.Enabled)
            {
                Console.WriteLine("> Adding Databse Context...");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(config.DatabaseOptions.ConnectionString)
                );

                services.AddTransient<IScrobbleRepository, ScrobbleRepository>();

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
