using System;

namespace Selector.Net;

public class Repeater<TNodeId>: BaseSource<TNodeId>, ISink<TNodeId>
{
    public IEnumerable<string> Topics { get; set; }

    private Type[] _types = Array.Empty<Type>();
    public IEnumerable<Type> Types => _types;

    public Task Consume(object obj)
    {
        Emit(obj);

        return Task.CompletedTask;
    }
}
