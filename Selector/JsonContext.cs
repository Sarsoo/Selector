using System.Text.Json.Serialization;
using SpotifyAPI.Web;

namespace Selector
{
    [JsonSerializable(typeof(CurrentlyPlayingDTO))]
    [JsonSerializable(typeof(TrackAudioFeatures))]
    [JsonSerializable(typeof(ListeningChangeEventArgs))]
    public partial class JsonContext: JsonSerializerContext
    {
    }
}
