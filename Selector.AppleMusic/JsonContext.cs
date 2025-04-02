using System.Text.Json.Serialization;
using Selector.AppleMusic.Model;

namespace Selector.AppleMusic;

[JsonSerializable(typeof(RecentlyPlayedTracksResponse))]
[JsonSerializable(typeof(TrackAttributes))]
[JsonSerializable(typeof(PlayParams))]
[JsonSerializable(typeof(Track))]
[JsonSerializable(typeof(AppleListeningChangeEventArgs))]
[JsonSerializable(typeof(AppleCurrentlyPlayingDTO))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class AppleJsonContext : JsonSerializerContext
{
}