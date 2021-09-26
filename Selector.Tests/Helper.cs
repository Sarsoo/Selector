using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SpotifyAPI.Web;

namespace Selector.Tests
{
    static class Helper
    {
        public static FullTrack FullTrack(string name, string album, List<string> artists)
        {
            return new FullTrack()
            {
                Name = name,
                Uri = name,
                Album = SimpleAlbum(album, artists),
                Artists = artists.Select(a => SimpleArtist(a)).ToList()
            };
        }

        public static FullTrack FullTrack(string name, string album, string artist)
        {
            return FullTrack(name, album, new List<string>() { artist });
        }

        public static SimpleAlbum SimpleAlbum(string name, List<string> artists)
        {
            return new SimpleAlbum()
            {
                Name = name,
                Uri = name,
                Artists = artists.Select(a => SimpleArtist(a)).ToList()
            };
        }

        public static SimpleAlbum SimpleAlbum(string name, string artist)
        {
            return SimpleAlbum(name, new List<string>() { artist });
        }

        public static SimpleArtist SimpleArtist(string name)
        {
            return new SimpleArtist()
            {
                Name = name,
                Uri = name
            };
        }

        public static CurrentlyPlaying CurrentlyPlaying(FullTrack track, bool isPlaying = true, string context = null)
        {
            return new CurrentlyPlaying()
            {
                Context = Context(context ?? track.Uri),
                IsPlaying = isPlaying,
                Item = track
            };
        }

        public static Context Context(string uri)
        {
            return new Context()
            {
                Uri = uri
            };
        }
    }
}
