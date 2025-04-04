﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Selector.Cache;
using Selector.Model.Authorisation;
using Selector.Spotify;
using Selector.Spotify.Watcher;

namespace Selector.Model.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSpotifyWatcher(this IServiceCollection services)
        {
            services.AddSingleton<ISpotifyWatcherFactory, SpotifyWatcherFactory>();

            return services;
        }

        public static void AddAuthorisationHandlers(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, WatcherIsOwnerAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, WatcherIsAdminAuthHandler>();

            services.AddScoped<IAuthorizationHandler, UserIsSelfAuthHandler>();
            services.AddSingleton<IAuthorizationHandler, UserIsAdminAuthHandler>();
        }

        public static IServiceCollection AddDBPlayCountPuller(this IServiceCollection services)
        {
            services.AddTransient<DBPlayCountPuller>();

            return services;
        }
    }
}