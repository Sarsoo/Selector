using System;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist;

public class ItemMutator<TNodeId> : BaseSinkSource<TNodeId, PlaylistChangeEventArgs>
{
    public Func<PlaylistTrack<IPlayableItem>, PlaylistTrack<IPlayableItem>> Func { get; set; }

    public ItemMutator(Func<PlaylistTrack<IPlayableItem>, PlaylistTrack<IPlayableItem>> func)
    {
        Func = func;
    }

    public override Task ConsumeType(PlaylistChangeEventArgs obj)
    {
        //obj.CurrentTracks = obj.CurrentTracks.Select(Func);
        obj.AddedTracks = obj.AddedTracks.Select(Func);
        obj.RemovedTracks = obj.RemovedTracks.Select(Func);

        Emit(obj);

        return Task.CompletedTask;
    }
}

