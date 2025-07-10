using System.Text.Json.Serialization;

namespace Selector.Cache;

[JsonSerializable(typeof(BaggageWrapper))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class CacheJsonContext : JsonSerializerContext
{
}