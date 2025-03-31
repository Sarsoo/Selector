using Selector.Spotify.ConfigFactory;

namespace Selector.Spotify.FactoryProvider
{
    public interface IRefreshTokenFactoryProvider
    {
        public Task<RefreshTokenFactory> GetFactory(string refreshToken);
    }
}