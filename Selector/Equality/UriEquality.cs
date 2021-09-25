using System;
using System.Linq;
using SpotifyAPI.Web;

namespace Selector {
    
    public class UriEquality: IEqualityChecker {
        public bool Track(FullTrack track1, FullTrack track2, bool includingAlbum = true)
        {
            var ret = includingAlbum ? Album(track1.Album, track2.Album) : true;

            return ret 
                && track1.Uri == track2.Uri
                && Enumerable.SequenceEqual(track1.Artists.Select(a => a.Uri), track2.Artists.Select(a => a.Uri));
        }
        public bool Album(FullAlbum album1, FullAlbum album2)
        {
            return album1.Uri == album2.Uri
                && Enumerable.SequenceEqual(album1.Artists.Select(a => a.Uri), album2.Artists.Select(a => a.Uri));
        }
        public bool Artist(FullArtist artist1, FullArtist artist2)
        {
            return artist1.Uri == artist2.Uri;
        }

        public bool Track(SimpleTrack track1, SimpleTrack track2)
        {
            return track1.Uri == track2.Uri
                && Enumerable.SequenceEqual(track1.Artists.Select(a => a.Uri), track2.Artists.Select(a => a.Uri));
        }
        public bool Album(SimpleAlbum album1, SimpleAlbum album2)
        {
            return album1.Uri == album2.Uri
                && Enumerable.SequenceEqual(album1.Artists.Select(a => a.Uri), album2.Artists.Select(a => a.Uri));
        }
        public bool Artist(SimpleArtist artist1, SimpleArtist artist2)
        {
            return artist1.Uri == artist2.Uri;
        }

        public bool Context(Context context1, Context context2)
        {
            return context1.Uri == context2.Uri;
        }
        public bool Device(Device device1, Device device2)
        {
            return device1.Id == device2.Id;
        }
    }
}