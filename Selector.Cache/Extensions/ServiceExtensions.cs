using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Selector.Cache.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddRedisServices(this IServiceCollection services, string connectionStr)
        {
            Console.WriteLine("> Configuring Redis...");

            if (string.IsNullOrWhiteSpace(connectionStr))
            {
                Console.WriteLine("> No Redis configuration string provided, exiting...");
                Environment.Exit(1);
            }

            var connMulti = ConnectionMultiplexer.Connect(connectionStr);
            services.AddSingleton(connMulti);
            services.AddTransient<IDatabaseAsync>(services => services.GetService<ConnectionMultiplexer>().GetDatabase());
            services.AddTransient<ISubscriber>(services => services.GetService<ConnectionMultiplexer>().GetSubscriber());
        }

        public static void AddCachingConsumerFactories(this IServiceCollection services)
        {
            services.AddTransient<IAudioFeatureInjectorFactory, AudioFeatureInjectorFactory>();
            services.AddTransient<IPlayCounterFactory, PlayCounterCachingFactory>();

            services.AddTransient<ICacheWriterFactory, CacheWriterFactory>();
            services.AddTransient<IPublisherFactory, PublisherFactory>();
        }
    }
}
