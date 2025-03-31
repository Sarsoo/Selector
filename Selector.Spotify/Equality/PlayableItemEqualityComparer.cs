using System.Diagnostics.CodeAnalysis;
using Selector.Extensions;
using SpotifyAPI.Web;

namespace Selector.Spotify.Equality
{
    public class PlayableItemEqualityComparer : IEqualityComparer<PlaylistTrack<IPlayableItem>>
    {
        public bool Equals(PlaylistTrack<IPlayableItem> x, PlaylistTrack<IPlayableItem> y)
        {
            return x.GetUri().Equals(y.GetUri());
        }

        public int GetHashCode([DisallowNull] PlaylistTrack<IPlayableItem> obj)
        {
            return obj.GetUri().GetHashCode();
        }
    }
}