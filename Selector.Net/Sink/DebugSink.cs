using System;
using Microsoft.Extensions.Logging;

namespace Selector.Net;

public class DebugSink<TNodeId>: ISink<TNodeId>
{
    public IEnumerable<string> Topics { get; set; }
    public TNodeId Id { get; set; }

    public ILogger<DebugSink<TNodeId>> Logger { get; set; }

    public Task Consume(object obj)
    {
        var type = obj.GetType();

        if (Logger is not null)
        {
            Logger.LogDebug("{} Received, {}", type, obj);
        }else
        {
            Console.WriteLine("{0} Received, {1}", type, obj);
        }

        return Task.CompletedTask;
    }
}

