using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace Selector.Spotify
{
    [JsonSerializable(typeof(CurrentlyPlayingDTO))]
    [JsonSerializable(typeof(TrackAudioFeatures))]
    [JsonSerializable(typeof(SpotifyListeningChangeEventArgs))]
    public partial class SpotifyJsonContext : JsonSerializerContext
    {
    }
}