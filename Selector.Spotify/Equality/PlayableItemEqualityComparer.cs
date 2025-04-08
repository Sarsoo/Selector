using System.Diagnostics.CodeAnalysis;
using Selector.Extensions;
using SpotifyAPI.Web;

namespace Selector.Spotify.Equality
{
    public class PlayableItemEqualityComparer : IEqualityComparer<PlaylistTrack<IPlayableItem>>
    {
        public bool Equals(PlaylistTrack<IPlayableItem>? x, PlaylistTrack<IPlayableItem>? y)
        {
            switch (x, y)
            {
                case (null, null):
                case (null, not null):
                case (not null, null):
                    return false;
                case var (left, right):
                    return left.GetUri().Equals(right.GetUri());
            }
        }

        public int GetHashCode([DisallowNull] PlaylistTrack<IPlayableItem> obj)
        {
            return obj.GetUri().GetHashCode();
        }
    }
}