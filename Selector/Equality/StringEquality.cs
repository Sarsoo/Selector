using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyAPI.Web;

namespace Selector {
    
    public class StringEquality: UriEquality {

        new public bool Track(FullTrack track1, FullTrack track2, bool includingAlbum = true)
        {
            var ret = includingAlbum ? Album(track1.Album, track2.Album): true;

            return ret
                && track1.Name == track2.Name
                && Enumerable.SequenceEqual(track1.Artists.Select(a => a.Name), track2.Artists.Select(a => a.Name));
        }

        new public bool Episode(FullEpisode ep1, FullEpisode ep2)
        {
            return ep1.Uri == ep2.Uri
                && Show(ep1.Show, ep2.Show);
        }

        new public bool Album(FullAlbum album1, FullAlbum album2)
        {
            return album1.Name == album2.Name
                && Enumerable.SequenceEqual(album1.Artists.Select(a => a.Name), album2.Artists.Select(a => a.Name));
        }

        new public bool Show(FullShow show1, FullShow show2)
        {
            return show1.Name == show2.Name
                && show1.Publisher == show2.Publisher;
        }

        new public bool Artist(FullArtist artist1, FullArtist artist2)
        {
            return artist1.Name == artist2.Name;
        }

        new public bool Track(SimpleTrack track1, SimpleTrack track2)
        {
            return track1.Name == track2.Name
                && Enumerable.SequenceEqual(track1.Artists.Select(a => a.Name), track2.Artists.Select(a => a.Name));
        }

        new public bool Episode(SimpleEpisode ep1, SimpleEpisode ep2)
        {
            return ep1.Name == ep2.Name;
        }

        new public bool Album(SimpleAlbum album1, SimpleAlbum album2)
        {
            return album1.Name == album2.Name
                && Enumerable.SequenceEqual(album1.Artists.Select(a => a.Name), album2.Artists.Select(a => a.Name));
        }

        new public bool Show(SimpleShow show1, SimpleShow show2)
        {
            return show1.Name == show2.Name
                && show1.Publisher == show2.Publisher;
        }
        
        new public bool Artist(SimpleArtist artist1, SimpleArtist artist2)
        {
            return artist1.Name == artist2.Name;
        }
    }
}