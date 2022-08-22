using System;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist;

public class ItemFilter<TNodeId> : BaseSinkSource<TNodeId, PlaylistChangeEventArgs>
{
    public Func<PlaylistTrack<IPlayableItem>, bool> Func { get; set; }

    public ItemFilter(Func<PlaylistTrack<IPlayableItem>, bool> func)
    {
        Func = func;
    }

    public override Task ConsumeType(PlaylistChangeEventArgs obj)
    {
        //obj.CurrentTracks = obj.CurrentTracks.Where(Func);
        obj.AddedTracks = obj.AddedTracks.Where(Func);
        obj.RemovedTracks = obj.RemovedTracks.Where(Func);

        Emit(obj);

        return Task.CompletedTask;
    }
}

