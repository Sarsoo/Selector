using System;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist
{
    public interface ICurrentItemListResolver
    {
        Task<IEnumerable<PlaylistTrack<IPlayableItem>>> GetCurrentItems();
    }
}

