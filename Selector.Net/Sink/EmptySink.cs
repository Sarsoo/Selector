using System;

namespace Selector.Net;

public class EmptySink<TNodeId, TObj>: BaseSingleSink<TNodeId, TObj>
{
    public override Task ConsumeType(TObj obj)
    {
        return Task.CompletedTask;
    }
}

