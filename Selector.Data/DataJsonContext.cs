using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Selector.Data;

[JsonSerializable(typeof(EndSong))]
[JsonSerializable(typeof(EndSong[]))]
public partial class DataJsonContext : JsonSerializerContext
{

}

