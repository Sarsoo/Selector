using System;
using QuikGraph;

namespace Selector.Net
{
    public interface IGraph<TNodeId>
    {
        IEnumerable<INode<TNodeId>> Nodes { get; }

        Task AddEdge(INode<TNodeId> from, INode<TNodeId> to);
        Task AddNode(INode<TNodeId> node);

        Task Sink(string topic, object obj);

        IEnumerable<ISource<TNodeId>> GetSources();

        IEnumerable<ISink<TNodeId>> GetSinks();
        IEnumerable<ISink<TNodeId, TSink>> GetSinks<TSink>();
    }
}

