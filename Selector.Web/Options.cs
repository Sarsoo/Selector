﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Selector.Web
{
    static class OptionsHelper {
        public static void ConfigureOptions(RootOptions options, IConfiguration config)
        {
            config.GetSection(RootOptions.Key).Bind(options);
            config.GetSection(FormatKeys(new[] { RootOptions.Key, RedisOptions.Key })).Bind(options.RedisOptions);
            config.GetSection(FormatKeys(new[] { RootOptions.Key, NowPlayingOptions.Key })).Bind(options.NowOptions);
            config.GetSection(FormatKeys(new[] { RootOptions.Key, PastOptions.Key })).Bind(options.PastOptions);
        }

        public static RootOptions ConfigureOptions(IConfiguration config)
        {
            var options = config.GetSection(RootOptions.Key).Get<RootOptions>();
            ConfigureOptions(options, config);
            return options;
        }  

        public static string FormatKeys(string[] args) => string.Join(":", args);
    }

    public class RootOptions
    {
        public const string Key = "Selector";

        /// <summary>
        /// Spotify client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Spotify app secret
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// Spotify callback for authentication
        /// </summary>
        public string SpotifyCallback { get; set; }
        public string LastfmClient { get; set; }
        public string LastfmSecret { get; set; }

        public TimeSpan CookieExpiry { get; set; } = TimeSpan.FromDays(30);

        public RedisOptions RedisOptions { get; set; } = new();
        public NowPlayingOptions NowOptions { get; set; } = new();
        public PastOptions PastOptions { get; set; } = new();

    }

    public class RedisOptions
    {
        public const string Key = "Redis";

        public bool Enabled { get; set; } = false;
        public string ConnectionString { get; set; }
    }

    public class JwtOptions
    {
        public const string _Key = "Jwt";


        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public TimeSpan Expiry { get; set; } = TimeSpan.FromDays(7);
    }

    public class AppleMusicOptions
    {
        public const string _Key = "AppleMusic";


        public string Key { get; set; }
        public string TeamId { get; set; }
        public string KeyId { get; set; }
        public TimeSpan? Expiry { get; set; } = null;
    }
}
