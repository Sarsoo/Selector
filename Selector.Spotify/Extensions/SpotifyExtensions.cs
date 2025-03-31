using SpotifyAPI.Web;

namespace Selector.Extensions
{
    public static class SpotifyExtensions
    {
        public static string DisplayString(this FullPlaylist playlist) => $"{playlist.Name}";

        public static string DisplayString(this FullTrack track) =>
            $"{track.Name} / {track.Album?.Name} / {track.Artists?.DisplayString()}";

        public static string DisplayString(this SimpleAlbum album) =>
            $"{album.Name} / {album.Artists?.DisplayString()}";

        public static string DisplayString(this SimpleArtist artist) => artist.Name;

        public static string DisplayString(this FullEpisode ep) => $"{ep.Name} / {ep.Show?.DisplayString()}";
        public static string DisplayString(this SimpleShow show) => $"{show.Name} / {show.Publisher}";


        public static string DisplayString(this CurrentlyPlayingContext currentPlaying)
        {
            if (currentPlaying.Item is FullTrack track)
            {
                return
                    $"{currentPlaying.IsPlaying}, {track.DisplayString()}, {currentPlaying.Device?.DisplayString() ?? "no device"}, {currentPlaying?.Context?.DisplayString() ?? "no context"}";
            }
            else if (currentPlaying.Item is FullEpisode episode)
            {
                return
                    $"{currentPlaying.IsPlaying}, {episode.DisplayString()}, {currentPlaying.Device?.DisplayString() ?? "no device"}";
            }
            else if (currentPlaying.Item is null)
            {
                return $"{currentPlaying.IsPlaying}, no item, {currentPlaying.Device?.DisplayString() ?? "no device"}";
            }
            else
            {
                throw new ArgumentException("Unknown playing type");
            }
        }

        public static string DisplayString(this Context context) => $"{context.Type}, {context.Uri}";

        public static string DisplayString(this Device device) =>
            $"{device.Name} ({device.Id}) {device.VolumePercent}%";

        public static string DisplayString(this TrackAudioFeatures feature) =>
            $"Acou. {feature.Acousticness}, Dance {feature.Danceability}, Energy {feature.Energy}, Instru. {feature.Instrumentalness}, Key {feature.Key}, Live {feature.Liveness}, Loud {feature.Loudness} dB, Mode {feature.Mode}, Speech {feature.Speechiness}, Tempo {feature.Tempo} BPM, Time Sig. {feature.TimeSignature}, Valence {feature.Valence}";

        public static string DisplayString(this IEnumerable<SimpleArtist> artists) =>
            string.Join(", ", artists.Select(a => a.DisplayString()));

        public static bool IsInstrumental(this TrackAudioFeatures feature) => feature.Instrumentalness > 0.5;
        public static bool IsLive(this TrackAudioFeatures feature) => feature.Liveness > 0.8f;
        public static bool IsSpokenWord(this TrackAudioFeatures feature) => feature.Speechiness > 0.66f;

        public static bool IsSpeechAndMusic(this TrackAudioFeatures feature) =>
            feature.Speechiness is >= 0.33f and <= 0.66f;

        public static bool IsNotSpeech(this TrackAudioFeatures feature) => feature.Speechiness < 0.33f;

        public static string GetUri(this IPlayableItem y)
        {
            if (y is FullTrack track)
            {
                return track.Uri;
            }
            else if (y is FullEpisode episode)
            {
                return episode.Uri;
            }
            else
            {
                throw new ArgumentException(nameof(y));
            }
        }

        public static string GetUri(this PlaylistTrack<IPlayableItem> y)
        {
            if (y.Track is FullTrack track)
            {
                return track.Uri;
            }
            else if (y.Track is FullEpisode episode)
            {
                return episode.Uri;
            }
            else
            {
                throw new ArgumentException(nameof(y.Track));
            }
        }
    }
}