using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpotifyAPI.Web;

namespace Selector.Equality
{
    public class PlayableItemEqualityComparer: IEqualityComparer<PlaylistTrack<IPlayableItem>>
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

