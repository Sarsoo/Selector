using System;
using QuikGraph;

namespace Selector.Net
{
    public class Graph<TNodeId> : IGraph<TNodeId>
    {
        protected AdjacencyGraph<INode<TNodeId>, SEdge<INode<TNodeId>>> graph { get; set; }
        private readonly object graphLock = new object();

        public Graph()
        {
            graph = new();
        }

        public IEnumerable<INode<TNodeId>> Nodes => graph.Vertices;

        public Task AddEdge(INode<TNodeId> from, INode<TNodeId> to)
        {
            lock(graphLock)
            {
                graph.AddVerticesAndEdge(new SEdge<INode<TNodeId>>(from, to));
            }

            if (from is ISource<TNodeId> fromSource)
            {
                fromSource.ReceiveHandler(SourceHandler);
            }

            if (to is ISource<TNodeId> toSource)
            {
                toSource.ReceiveHandler(SourceHandler);
            }

            return Task.CompletedTask;
        }

        public Task AddNode(INode<TNodeId> node)
        {
            lock (graphLock)
            {
                graph.AddVertex(node);
            }

            if (node is ISource<TNodeId> source)
            {
                source.ReceiveHandler(SourceHandler);
            }

            return Task.CompletedTask;
        }

        private async void SourceHandler(ISource<TNodeId> sender, object obj,
            IEnumerable<TNodeId> nodeWhitelist = null, IEnumerable<TNodeId> nodeBlacklist = null)
        {
            if(nodeWhitelist is not null && nodeBlacklist is not null && nodeWhitelist.Any() && nodeBlacklist.Any())
            {
                throw new ArgumentException("Cannot provide whitelist and blacklist, at most one");
            }

            if (graph.TryGetOutEdges(sender, out var edges))
            {
                foreach (var edge in edges)
                {
                    if (edge.Target is ISink<TNodeId> sink)
                    {
                        if(nodeWhitelist is not null && nodeWhitelist.Any())
                        {
                            if(nodeWhitelist.Contains(sink.Id))
                            {
                                await sink.Consume(obj);
                            }
                        }
                        else if (nodeBlacklist is not null && nodeBlacklist.Any())
                        {
                            if (!nodeBlacklist.Contains(sink.Id))
                            {
                                await sink.Consume(obj);
                            }
                        }
                        else
                        {
                            await sink.Consume(obj);
                        }
                    }
                }
            }
        }

        public IEnumerable<ISink<TNodeId>> GetSinks()
        {
            foreach (var node in graph.Vertices)
            {
                if (node is ISink<TNodeId> sink)
                {
                    yield return sink;
                }
            }
        }

        public IEnumerable<ISink<TNodeId, TSink>> GetSinks<TSink>()
        {
            foreach (var node in graph.Vertices)
            {
                if (node is ISink<TNodeId, TSink> sink)
                {
                    yield return sink;
                }
            }
        }

        public IEnumerable<ISource<TNodeId>> GetSources()
        {
            foreach (var node in graph.Vertices)
            {
                if (node is ISource<TNodeId> source)
                {
                    yield return source;
                }
            }
        }

        public async Task Sink(string topic, object obj)
        {
            foreach (var sink in GetSinks())
            {
                if (sink.Topics.Contains(topic))
                {
                    await sink.Consume(obj);
                }
            }
        }
    }
}

