﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Selector.Spotify.Consumer.Factory;
using StackExchange.Redis;

namespace Selector.Cache.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRedisServices(this IServiceCollection services, string connectionStr)
        {
            Console.WriteLine("> Configuring Redis...");

            if (string.IsNullOrWhiteSpace(connectionStr))
            {
                Console.WriteLine("> No Redis configuration string provided, exiting...");
                Environment.Exit(1);
            }

            var connMulti = ConnectionMultiplexer.Connect(connectionStr);
            services.AddSingleton(connMulti);
            services.AddTransient<IDatabaseAsync>(
                s => s.GetService<ConnectionMultiplexer>().GetDatabase());
            services.AddTransient<ISubscriber>(s =>
                s.GetService<ConnectionMultiplexer>().GetSubscriber());

            return services;
        }

        public static IServiceCollection AddCachingConsumerFactories(this IServiceCollection services)
        {
            // services.AddTransient<IAudioFeatureInjectorFactory, CachingAudioFeatureInjectorFactory>();
            // services.AddTransient<CachingAudioFeatureInjectorFactory>();
            services.AddTransient<IPlayCounterFactory, PlayCounterCachingFactory>();
            services.AddTransient<PlayCounterCachingFactory>();

            services.AddTransient<ICacheWriterFactory, CacheWriterFactory>();
            services.AddTransient<CacheWriterFactory>();
            services.AddTransient<IPublisherFactory, PublisherFactory>();
            services.AddTransient<PublisherFactory>();

            return services;
        }

        public static IServiceCollection AddCachingSpotify(this IServiceCollection services)
        {
            services.AddSingleton<AudioFeaturePuller>();

            return services;
        }

        public static IServiceCollection AddCachingLastFm(this IServiceCollection services)
        {
            services.AddSingleton<PlayCountPuller>();

            return services;
        }
    }
}