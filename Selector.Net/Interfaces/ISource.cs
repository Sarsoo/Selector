using System;
namespace Selector.Net
{
    public delegate void Emit<T>(ISource<T> sender, object obj, IEnumerable<T> nodeWhitelist = null, IEnumerable<T> nodeBlacklist = null);
    public delegate void Emit<TNodeId, TObj>(ISource<TNodeId, TObj> sender, object obj, IEnumerable<T> nodeWhitelist = null, IEnumerable<T> nodeBlacklist = null);

    public interface ISource<TNodeId> : INode<TNodeId>
    {
        void ReceiveHandler(Emit<TNodeId> handler);
    }

    public interface ISource<TNodeId, TObj> : INode<TNodeId>
    {
        void ReceiveHandler(Emit<TNodeId, TObj> handler);
    }
}

