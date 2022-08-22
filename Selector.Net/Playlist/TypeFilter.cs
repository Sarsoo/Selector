using System;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist;

public enum PlayableItemType
{
    Track, Episode
}

public class TypeFilter<TNodeId> : BaseSinkSource<TNodeId, IEnumerable<PlaylistTrack<IPlayableItem>>>
{
    public PlayableItemType FilterType { get; set; }

    public TypeFilter(PlayableItemType filterType)
    {
        FilterType = filterType;
    }

    public override Task ConsumeType(IEnumerable<PlaylistTrack<IPlayableItem>> tracks)
    {
        switch (FilterType)
        {
            case PlayableItemType.Track:

                Emit(tracks.Where(i => i.Track is FullTrack));

                break;
            case PlayableItemType.Episode:

                Emit(tracks.Where(i => i.Track is FullEpisode));

                break;
        }


        return Task.CompletedTask;
    }
}

