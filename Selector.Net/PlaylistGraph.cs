using System;
namespace Selector.Net.Playlist;

public class PlaylistGraph<TNodeId>
{
    private IGraph<TNodeId> graph { get; set; }

    public PlaylistGraph(PlaylistFilterConfig filterConfig)
    {
        graph = new Graph<TNodeId>();

        var entryFilter = new PlaylistFilter<TNodeId>(filterConfig)
        {
            Topics = new[] { "track-entry" }
        };
    }
}

