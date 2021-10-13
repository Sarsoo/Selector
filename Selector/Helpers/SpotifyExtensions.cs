using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using SpotifyAPI.Web;

namespace Selector
{
    public static class SpotifyExtensions
    {
        public static string DisplayString(this FullTrack track) => $"{track.Name} / {track.Album.Name} / {track.Artists.DisplayString()}";
        public static string DisplayString(this SimpleAlbum album) => $"{album.Name} / {album.Artists.DisplayString()}";
        public static string DisplayString(this SimpleArtist artist) => artist.Name;

        public static string DisplayString(this FullEpisode ep) => $"{ep.Name} / {ep.Show.DisplayString()}";
        public static string DisplayString(this SimpleShow show) => $"{show.Name} / {show.Publisher}";


        public static string DisplayString(this CurrentlyPlayingContext currentPlaying) => $"{currentPlaying.IsPlaying}, {currentPlaying.Item}, {currentPlaying.Device.DisplayString()}";
        public static string DisplayString(this Context context) => $"{context.Type}, {context.Uri}";
        public static string DisplayString(this Device device) => $"{device.Name} ({device.Id}) {device.VolumePercent}%";
        public static string DisplayString(this TrackAudioFeatures feature) => $"Acou. {feature.Acousticness}, Dance {feature.Danceability}, Energy {feature.Energy}, Instru. {feature.Instrumentalness}, Key {feature.Key}, Live {feature.Liveness}, Loud {feature.Loudness}, Mode {feature.Mode}, Speech {feature.Speechiness}, Tempo {feature.Tempo}, Valence {feature.Valence}";

        public static string DisplayString(this IEnumerable<SimpleArtist> artists) => string.Join(", ", artists.Select(a => a.DisplayString()));
    }
}
