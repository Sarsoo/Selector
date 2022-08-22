using System;

namespace Selector.Net;

public abstract class BaseSinkSource<TNodeId, TObj> : BaseSingleSink<TNodeId, TObj>, ISource<TNodeId>
{
    protected Emit<TNodeId> EmitHandler { get; set; }

    public void ReceiveHandler(Emit<TNodeId> handler)
    {
        EmitHandler = handler;
    }

    protected void Emit(object obj)
    {
        EmitHandler(this, obj);
    }
}

