using System;

namespace Selector.Net;

public abstract class BaseSource<TNodeId>: BaseNode<TNodeId>, ISource<TNodeId>
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

