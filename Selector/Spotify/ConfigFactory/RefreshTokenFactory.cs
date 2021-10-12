using System.Threading.Tasks;

using SpotifyAPI.Web;


namespace Selector
{
    /// <summary>
    /// Get config from a refresh token
    /// </summary>
    public class RefreshTokenFactory : ISpotifyConfigFactory
    {
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }
        private string RefreshToken { get; set; }

        public RefreshTokenFactory(string clientId, string clientSecret, string refreshToken) {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RefreshToken = refreshToken;
        }

        public async Task<SpotifyClientConfig> GetConfig()
        {
            var refreshed = await new OAuthClient()
                .RequestToken(new AuthorizationCodeRefreshRequest(ClientId, ClientSecret, RefreshToken));

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(ClientId, ClientSecret, new(){
                    AccessToken = refreshed.AccessToken,
                    TokenType = refreshed.TokenType,
                    ExpiresIn = refreshed.ExpiresIn,
                    Scope = refreshed.Scope,
                    RefreshToken = refreshed.RefreshToken,
                    CreatedAt = refreshed.CreatedAt
                }));

            return config;
        }
    }
}
