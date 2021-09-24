using System;
using SpotifyAPI.Web;

namespace Selector
{
    public class Watcher
    {
        private readonly IPlayerClient spotifyClient;

        public Watcher(IPlayerClient spotifyClient) {
            this.spotifyClient = spotifyClient;
        }
    }
}
