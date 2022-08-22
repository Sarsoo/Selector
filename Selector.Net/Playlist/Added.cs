using System;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist;

public enum AddedType
{
    Since, Before
}


public class Added<TNodeId> : BaseSinkSource<TNodeId, PlaylistChangeEventArgs>
{
    public DateTime Threshold { get; set; }
    public bool IncludeNull { get; set; }

    public AddedType Operator { get; set; } = AddedType.Since;

    private IEnumerable<PlaylistTrack<IPlayableItem>> Filter(IEnumerable<PlaylistTrack<IPlayableItem>> tracks)
    {
        return tracks.Where(t =>
        {
            if (t.AddedAt is not null)
            {
                switch (Operator)
                {
                    case AddedType.Since:
                        return t.AddedAt.Value > Threshold;
                    case AddedType.Before:
                        return t.AddedAt.Value < Threshold;
                }
            }
            else
            {
                if (IncludeNull)
                {
                    return true;
                }
            }

            return false;
        });
    }

    public override Task ConsumeType(PlaylistChangeEventArgs obj)
    {
        //obj.CurrentTracks = Filter(obj.CurrentTracks);
        obj.AddedTracks = Filter(obj.AddedTracks);
        obj.RemovedTracks = Filter(obj.RemovedTracks);

        Emit(obj);

        return Task.CompletedTask;
    }
}

