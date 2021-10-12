using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Selector
{
    public interface ISpotifyConfigFactory
    {
        public Task<SpotifyClientConfig> GetConfig();
    }
}
