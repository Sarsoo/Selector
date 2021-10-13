using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpotifyAPI.Web;

namespace Selector
{
    public class PlayerTimeline 
        : BaseTimeline<CurrentlyPlayingContext>, ITrackTimeline<CurrentlyPlayingContext>
    {
        public IEqual EqualityChecker { get; set; }

        public override void Add(CurrentlyPlayingContext item) => Add(item, DateHelper.FromUnixMilli(item.Timestamp));

        public CurrentlyPlayingContext Get(FullTrack track) 
            => GetAll(track)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(FullTrack track)
            => GetAllTimelineItems(track)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(FullTrack track)
            => Recent
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack, track));

        public CurrentlyPlayingContext Get(FullEpisode ep)
            => GetAll(ep)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(FullEpisode ep)
            => GetAllTimelineItems(ep)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(FullEpisode ep)
            => Recent
                .Where(i => i.Item.Item is FullEpisode iterEp
                        && EqualityChecker.IsEqual(iterEp, ep));

        public CurrentlyPlayingContext Get(SimpleAlbum album)
            => GetAll(album)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(SimpleAlbum album)
            => GetAllTimelineItems(album)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(SimpleAlbum album)
            => Recent
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack.Album, album));

        public CurrentlyPlayingContext Get(SimpleArtist artist)
            => GetAll(artist)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(SimpleArtist artist)
            => GetAllTimelineItems(artist)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(SimpleArtist artist)
            => Recent
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack.Artists[0], artist));

        public CurrentlyPlayingContext Get(Device device)
            => GetAll(device)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(Device device)
            => GetAllTimelineItems(device)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(Device device) 
            => Recent
                .Where(i => EqualityChecker.IsEqual(i.Item.Device, device));

        public CurrentlyPlayingContext Get(Context context)
            => GetAll(context)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(Context context)
            => GetAllTimelineItems(context)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(Context context)
            => Recent
                .Where(i => EqualityChecker.IsEqual(i.Item.Context, context));
    }
}
