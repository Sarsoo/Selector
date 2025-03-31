using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.Spotify.Timeline
{
    public class AnalysedTrackTimeline
        : Timeline<AnalysedTrack>, ITrackTimeline<AnalysedTrack>
    {
        public IEqual EqualityChecker { get; set; }

        public AnalysedTrack Get(FullTrack track)
            => GetAll(track)
                .LastOrDefault();

        public IEnumerable<AnalysedTrack> GetAll(FullTrack track)
            => GetAllTimelineItems(track)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<AnalysedTrack>> GetAllTimelineItems(FullTrack track)
            => Recent
                .Where(i => EqualityChecker.IsEqual(i.Item.Track, track));

        public AnalysedTrack Get(SimpleAlbum album)
            => GetAll(album)
                .LastOrDefault();

        public IEnumerable<AnalysedTrack> GetAll(SimpleAlbum album)
            => GetAllTimelineItems(album)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<AnalysedTrack>> GetAllTimelineItems(SimpleAlbum album)
            => Recent
                .Where(i => EqualityChecker.IsEqual(i.Item.Track.Album, album));

        public AnalysedTrack Get(SimpleArtist artist)
            => GetAll(artist)
                .LastOrDefault();

        public IEnumerable<AnalysedTrack> GetAll(SimpleArtist artist)
            => GetAllTimelineItems(artist)
                .Select(t => t.Item);

        public IEnumerable<TimelineItem<AnalysedTrack>> GetAllTimelineItems(SimpleArtist artist)
            => Recent
                .Where(i => EqualityChecker.IsEqual(i.Item.Track.Artists[0], artist));
    }
}