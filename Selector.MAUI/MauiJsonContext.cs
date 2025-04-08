using System.Text.Json.Serialization;
using Selector.MAUI.Services;

namespace Selector.MAUI;

[JsonSerializable(typeof(SelectorNetClient.TokenNetworkResponse))]
public partial class MauiJsonContext : JsonSerializerContext
{
}