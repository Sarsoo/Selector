using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpotifyAPI.Web;

namespace Selector
{
    public class PlayerTimeline 
        : ITimeline<CurrentlyPlayingContext>, 
          IEnumerable<TimelineItem<CurrentlyPlayingContext>>
    {

        private List<TimelineItem<CurrentlyPlayingContext>> recentlyPlayed = new();
        public IEqual EqualityChecker { get; set; }
        public bool SortOnBackDate { get; set; } = true;
        public int Count { get => recentlyPlayed.Count; }

        private int? max = 1000;
        public int? MaxSize { 
            get => max; 
            set {
                if(value is null)
                {
                    max = value;
                }
                else 
                {
                    max = Math.Max(1, (int) value);   
                }
            }
        }

        public void Add(CurrentlyPlayingContext item) => Add(item, DateHelper.FromUnixMilli(item.Timestamp));
        public void Add(CurrentlyPlayingContext item, DateTime timestamp)
        {
            recentlyPlayed.Add(TimelineItem<CurrentlyPlayingContext>.From(item, timestamp));

            if (timestamp < recentlyPlayed.Last().Time && SortOnBackDate)
            {
                Sort();
            }

            CheckSize();
        }

        public void Sort()
        {
            recentlyPlayed = recentlyPlayed
                                .OrderBy(i => i.Time)
                                .ToList();
        }

        private void CheckSize()
        {
            if (MaxSize is int maxSize && Count > maxSize) {
                recentlyPlayed.RemoveRange(0, Count - maxSize);
            }
        }

        public void Clear() => recentlyPlayed.Clear();

        public CurrentlyPlayingContext Get() 
            => recentlyPlayed.Last().Item;

        public CurrentlyPlayingContext Get(DateTime at) 
            => GetTimelineItem(at)?.Item;
        public TimelineItem<CurrentlyPlayingContext> GetTimelineItem(DateTime at) 
            => recentlyPlayed
                .Where(i => i.Time <= at).LastOrDefault();

        public CurrentlyPlayingContext Get(FullTrack track) 
            => GetAll(track)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(FullTrack track)
            => GetAllTimelineItems(track)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(FullTrack track)
            => recentlyPlayed
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack, track));

        public CurrentlyPlayingContext Get(FullEpisode ep)
            => GetAll(ep)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(FullEpisode ep)
            => GetAllTimelineItems(ep)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(FullEpisode ep)
            => recentlyPlayed
                .Where(i => i.Item.Item is FullEpisode iterEp
                        && EqualityChecker.IsEqual(iterEp, ep));

        public CurrentlyPlayingContext Get(SimpleAlbum album)
            => GetAll(album)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(SimpleAlbum album)
            => GetAllTimelineItems(album)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(SimpleAlbum album)
            => recentlyPlayed
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack.Album, album));

        public CurrentlyPlayingContext Get(SimpleArtist artist)
            => GetAll(artist)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(SimpleArtist artist)
            => GetAllTimelineItems(artist)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(SimpleArtist artist)
            => recentlyPlayed
                .Where(i => i.Item.Item is FullTrack iterTrack
                        && EqualityChecker.IsEqual(iterTrack.Artists[0], artist));

        public CurrentlyPlayingContext Get(Device device)
            => GetAll(device)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(Device device)
            => GetAllTimelineItems(device)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(Device device) 
            => recentlyPlayed
                .Where(i => EqualityChecker.IsEqual(i.Item.Device, device));

        public CurrentlyPlayingContext Get(Context context)
            => GetAll(context)
                .LastOrDefault();

        public IEnumerable<CurrentlyPlayingContext> GetAll(Context context)
            => GetAllTimelineItems(context)
                .Select(t => t.Item);

        private IEnumerable<TimelineItem<CurrentlyPlayingContext>> GetAllTimelineItems(Context context)
            => recentlyPlayed
                .Where(i => EqualityChecker.IsEqual(i.Item.Context, context));

        public IEnumerator<TimelineItem<CurrentlyPlayingContext>> GetEnumerator() => recentlyPlayed.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
