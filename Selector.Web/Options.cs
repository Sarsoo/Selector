using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Selector.Web
{
    public static class OptionsHelper {
        public static void ConfigureOptions(RootOptions options, IConfiguration config)
        {
            config.GetSection(RootOptions.Key).Bind(options);
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
    }
}
