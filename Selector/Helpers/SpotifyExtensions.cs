using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using SpotifyAPI.Web;

namespace Selector.Helpers
{
    public static class SpotifyExtensions
    {
        public static string ToString(this FullTrack track) => $"{track.Name} - {track.Album.Name} - {track.Artists}";
        public static string ToString(this SimpleAlbum album) => $"{album.Name} - {album.Artists}";
        public static string ToString(this SimpleArtist artist) => $"{artist.Name}";

        public static string ToString(this FullEpisode ep) => $"{ep.Name} - {ep.Show}";
        public static string ToString(this SimpleShow show) => $"{show.Name} - {show.Publisher}";

        public static string ToString(this CurrentlyPlayingContext context) => $"{context.IsPlaying}, {context.Item}, {context.Device}";
        public static string ToString(this Device device) => $"{device.Id}: {device.Name} {device.VolumePercent}%";

        public static string ToString(this IEnumerable<SimpleArtist> artists) => string.Join("/", artists.Select(a => a.Name));
    }
}
