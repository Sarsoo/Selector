using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selector.Spotify.ConfigFactory;
using SpotifyAPI.Web;

namespace Selector.Spotify.FactoryProvider
{
    public class CachingRefreshTokenFactoryProvider : RefreshTokenFactoryProvider
    {
        protected readonly ILogger<CachingRefreshTokenFactoryProvider> Logger;

        public CachingRefreshTokenFactoryProvider(IOptions<SpotifyAppCredentials> credentials,
            ILogger<CachingRefreshTokenFactoryProvider> logger) : base(credentials)
        {
            Logger = logger;
        }

        protected Dictionary<string, RefreshTokenFactory> Configs = new();

        public RefreshTokenFactory GetUserConfig(string userId) => Configs.ContainsKey(userId) ? Configs[userId] : null;

        public override async Task<RefreshTokenFactory> GetFactory(string refreshToken)
        {
            var configProvider = await base.GetFactory(refreshToken);
            var newConfig = await configProvider.GetConfig();

            var client = new SpotifyClient(newConfig);
            var userDetails = await client.UserProfile.Current();

            if (Configs.ContainsKey(userDetails.Id))
            {
                return Configs[userDetails.Id];
            }
            else
            {
                Logger.LogDebug("New user token factory added [{spotify_name}]", userDetails.DisplayName);
                Configs[userDetails.Id] = configProvider;
                return configProvider;
            }
        }
    }
}