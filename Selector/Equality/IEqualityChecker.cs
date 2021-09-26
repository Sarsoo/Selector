using System;
using SpotifyAPI.Web;

namespace Selector {

    public interface IEqualityChecker {
        public bool Track(FullTrack track1, FullTrack track2, bool includingAlbum);
        public bool Episode(FullEpisode ep1, FullEpisode ep2);
        public bool Album(FullAlbum album1, FullAlbum album2);
        public bool Artist(FullArtist artist1, FullArtist artist2);

        public bool Track(SimpleTrack track1, SimpleTrack track2);
        public bool Episode(SimpleEpisode ep1, SimpleEpisode ep2);
        public bool Album(SimpleAlbum album1, SimpleAlbum album2);
        public bool Artist(SimpleArtist artist1, SimpleArtist artist2);

        public bool Context(Context context1, Context context2);
        public bool Device(Device device1, Device device2);
    }
}