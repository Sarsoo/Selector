using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;


namespace Selector
{
    public class CachingRefreshTokenFactoryProvider : RefreshTokenFactoryProvider
    {
        protected Dictionary<string, RefreshTokenFactory> Configs = new();

        public RefreshTokenFactory GetUserConfig(string userId) => Configs.ContainsKey(userId) ? Configs[userId] : null;

        new public async Task<RefreshTokenFactory> GetFactory(string refreshToken)
        {
            var configProvider = await base.GetFactory(refreshToken);
            var newConfig = await configProvider.GetConfig();

            var client = new SpotifyClient(newConfig);
            var userDetails = await client.UserProfile.Current();

            if(Configs.ContainsKey(userDetails.Id))
            {
                return Configs[userDetails.Id];
            }
            else 
            {
                Configs[userDetails.Id] = configProvider;
                return configProvider;
            }
        }
    }
}
