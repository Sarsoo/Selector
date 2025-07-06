using System.Text.Json.Serialization;
using Selector.MAUI.Services;

namespace Selector.MAUI;

[JsonSerializable(typeof(SelectorNetClient.TokenNetworkResponse))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class MauiJsonContext : JsonSerializerContext
{
}