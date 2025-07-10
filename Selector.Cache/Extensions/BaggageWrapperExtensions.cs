using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using StackExchange.Redis;

namespace Selector.Cache.Extensions;

public static class BaggageWrapperExtensions
{
    public static BaggageWrapper BaggageWrapped<T>(this T obj, JsonTypeInfo<T> serializer)
    {
        var ret = new BaggageWrapper
        {
            Message = JsonSerializer.Serialize(obj, serializer)
        };

        var propagationContext = new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current);

        Propagators.DefaultTextMapPropagator.Inject(
            propagationContext,
            ret.Headers,
            (headers, key, value) => headers[key] = value
        );

        return ret;
    }

    public static string SerialiseBaggageWrapped<T>(this T obj, JsonTypeInfo<T> serializer)
        => JsonSerializer.Serialize(obj.BaggageWrapped(serializer), CacheJsonContext.Default.BaggageWrapper);

    public static DeserializedActivity<T> DeserialiseBaggageWrapped<T>(this string json, JsonTypeInfo<T> serializer,
        string channel)
    {
        var wrapper = JsonSerializer.Deserialize(json, CacheJsonContext.Default.BaggageWrapper);

        var propagationContext = Propagators.DefaultTextMapPropagator.Extract(
            default,
            wrapper.Headers,
            (headers, key) => headers.TryGetValue(key, out var value) ? new[] { value } : Array.Empty<string>()
        );

        var activity = Trace.Tracer.StartActivity("receive - " + channel, ActivityKind.Consumer,
            propagationContext.ActivityContext);
        Baggage.Current = propagationContext.Baggage;

        var obj = JsonSerializer.Deserialize(wrapper.Message, serializer);

        return new DeserializedActivity<T>
        {
            Object = obj,
            Activity = activity,
        };
    }

    public static DeserializedActivity<T> DeserialiseBaggageWrapped<T>(this ChannelMessage message,
        JsonTypeInfo<T> serializer) =>
        DeserialiseBaggageWrapped(message.Message, serializer, message.Channel);
}