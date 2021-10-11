using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace Selector
{
    public interface ISpotifyClientFactory
    {
        public Task<SpotifyClientConfig> GetConfig();
    }
}
