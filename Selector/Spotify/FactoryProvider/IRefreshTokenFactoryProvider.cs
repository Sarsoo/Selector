using System.Threading.Tasks;

namespace Selector
{
    public interface IRefreshTokenFactoryProvider
    {
        public void Initialise(string clientId, string clientSecret);
        public bool Initialised { get; }
        public Task<RefreshTokenFactory> GetFactory(string refreshToken);
    }
}
