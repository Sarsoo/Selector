using System;
namespace Selector.Net
{
    public interface ISink<TNodeId> : INode<TNodeId>
    {
        IEnumerable<string> Topics { get; set; }

        Task Consume(object obj);
    }

    /// <summary>
    /// Not a node, just callback handler
    /// </summary>
    /// <typeparam name="TObj"></typeparam>
    public interface ITypeSink<TObj>
    {
        Task ConsumeType(TObj obj);
    }

    public interface ISink<TNodeId, TObj> : ISink<TNodeId>, ITypeSink<TObj>
    {
        //Task ConsumeType(TObj obj);
    }
}

