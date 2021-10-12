using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;


namespace Selector
{
    /// <summary>
    /// Get config from a refresh token
    /// </summary>
    public class RefreshTokenFactoryProvider : IRefreshTokenFactoryProvider
    {
        protected string ClientId { get; set; }
        protected string ClientSecret { get; set; }

        public void Initialise(string clientId, string clientSecret){
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public bool Initialised => string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret);

        public Task<RefreshTokenFactory> GetFactory(string refreshToken)
        {
            if(!Initialised) throw new InvalidOperationException("Factory not initialised");
            if(string.IsNullOrEmpty(refreshToken)) throw new ArgumentException("Null or empty refresh key provided");

            return Task.FromResult(new RefreshTokenFactory(ClientId, ClientSecret, refreshToken));
        }
    }
}
