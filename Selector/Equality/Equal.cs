using System;
using System.Collections.Generic;
using System.Text;
using SpotifyAPI.Web;

namespace Selector
{
    public class Equal : IEqual
    {
        private Dictionary<Type, object> comps;

        public static Equal String() {
            return new Equal(){
                comps = new Dictionary<Type, object>(){
                    {typeof(FullTrack), new FullTrackStringComparer()},
                    {typeof(FullEpisode), new FullEpisodeStringComparer()},
                    {typeof(FullAlbum), new FullAlbumStringComparer()},
                    {typeof(FullShow), new FullShowStringComparer()},
                    {typeof(FullArtist), new FullArtistStringComparer()},
                    
                    {typeof(SimpleTrack), new SimpleTrackStringComparer()},
                    {typeof(SimpleEpisode), new SimpleEpisodeStringComparer()},
                    {typeof(SimpleAlbum), new SimpleAlbumStringComparer()},
                    {typeof(SimpleShow), new SimpleShowStringComparer()},
                    {typeof(SimpleArtist), new SimpleArtistStringComparer()},
                }
            };
        }

        public bool IsEqual<T>(T item, T other)
        {
            if (comps.ContainsKey(typeof(T)))
            {
                var comp = (IEqualityComparer<T>) comps[typeof(T)];
                return comp.Equals(item, other);
            }
            else
            {
                throw new ArgumentException($"{typeof(T)} had no corresponding equality checker");
            }
        }
    }
}
