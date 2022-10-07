using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.CLI.Extensions
{
    public static class SpotifyExtensions
    {
        public static async Task<(FullPlaylist, IEnumerable<PlaylistTrack<IPlayableItem>>)> GetPopulated(this ISpotifyClient client, string playlistId, ILogger logger = null)
        {
            try
            {
                var playlist = await client.Playlists.Get(playlistId);
                var items = await client.Paginate(playlist.Tracks).ToListAsync();

                return (playlist, items);
            }
            catch (APIUnauthorizedException e)
            {
                logger?.LogDebug("Unauthorised error: [{message}] (should be refreshed and retried?)", e.Message);
                throw e;
            }
            catch (APITooManyRequestsException e)
            {
                logger?.LogDebug("Too many requests error: [{message}]", e.Message);
                throw e;
            }
            catch (APIException e)
            {
                logger?.LogDebug("API error: [{message}]", e.Message);
                throw e;
            }
        }
    }
}

