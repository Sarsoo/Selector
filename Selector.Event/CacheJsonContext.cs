using System.Text.Json.Serialization;

namespace Selector.Events
{
    [JsonSerializable(typeof(LastfmChange))]
    [JsonSerializable(typeof(SpotifyLinkChange))]
    [JsonSerializable(typeof((string, CurrentlyPlayingDTO)))]
    public partial class CacheJsonContext: JsonSerializerContext
    {
    }
}
