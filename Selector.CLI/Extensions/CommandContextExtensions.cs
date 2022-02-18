﻿using IF.Lastfm.Core.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Selector.Model;

namespace Selector.CLI.Extensions
{
    public static class CommandContextExtensions
    {
        public static CommandContext WithConfig(this CommandContext context)
        {
            var configBuild = new ConfigurationBuilder();
            configBuild.AddJsonFile("appsettings.json", optional: true)
                       .AddJsonFile("appsettings.Development.json", optional: true)
                       .AddJsonFile("appsettings.Production.json", optional: true);
            context.Config = configBuild.Build().ConfigureOptions();

            return context;
        }

        public static CommandContext WithLogger(this CommandContext context)
        {
            context.Logger = LoggerFactory.Create(builder =>
            {
                //builder.AddConsole(a => a.);
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                });
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            return context;
        }

        public static CommandContext WithDb(this CommandContext context)
        {
            if (context.Config is null)
            {
                context.WithConfig();
            }

            context.DatabaseConfig = new DbContextOptionsBuilder<ApplicationDbContext>();
            context.DatabaseConfig.UseNpgsql(context.Config.DatabaseOptions.ConnectionString);

            return context;
        }

        public static CommandContext WithLastfmApi(this CommandContext context)
        {
            if (context.Config is null)
            {
                context.WithConfig();
            }

            context.LastFmClient = new LastfmClient(new LastAuth(context.Config.LastfmClient, context.Config.LastfmSecret));

            return context;
        }
    }
}