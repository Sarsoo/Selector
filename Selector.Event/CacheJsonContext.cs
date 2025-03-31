using System.Text.Json.Serialization;
using Selector.Spotify;

namespace Selector.Events
{
    [JsonSerializable(typeof(LastfmChange))]
    [JsonSerializable(typeof(SpotifyLinkChange))]
    [JsonSerializable(typeof(AppleMusicLinkChange))]
    [JsonSerializable(typeof((string, CurrentlyPlayingDTO)))]
    public partial class CacheJsonContext : JsonSerializerContext
    {
    }
}