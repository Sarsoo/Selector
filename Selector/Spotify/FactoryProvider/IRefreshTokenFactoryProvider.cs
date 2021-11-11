using System.Threading.Tasks;

namespace Selector
{
    public interface IRefreshTokenFactoryProvider
    {
        public Task<RefreshTokenFactory> GetFactory(string refreshToken);
    }
}
