using Microsoft.Extensions.Options;
using Selector.Spotify.ConfigFactory;

namespace Selector.Spotify.FactoryProvider
{
    /// <summary>
    /// Get config from a refresh token
    /// </summary>
    public class RefreshTokenFactoryProvider : IRefreshTokenFactoryProvider
    {
        protected readonly SpotifyAppCredentials Credentials;

        public RefreshTokenFactoryProvider(IOptions<SpotifyAppCredentials> credentials)
        {
            Credentials = credentials.Value;
        }

        public virtual Task<RefreshTokenFactory> GetFactory(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentException("Null or empty refresh key provided");

            return Task.FromResult(
                new RefreshTokenFactory(Credentials.ClientId, Credentials.ClientSecret, refreshToken));
        }
    }
}