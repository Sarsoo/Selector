using SpotifyAPI.Web;

namespace Selector.Spotify.Timeline
{
    public interface ITrackTimeline<T> : ITimeline<T>
    {
        public T? Get(FullTrack track);
        public IEnumerable<T> GetAll(FullTrack track);
        public IEnumerable<TimelineItem<T>> GetAllTimelineItems(FullTrack track);

        public T? Get(SimpleAlbum album);
        public IEnumerable<T> GetAll(SimpleAlbum album);
        public IEnumerable<TimelineItem<T>> GetAllTimelineItems(SimpleAlbum album);

        public T? Get(SimpleArtist artist);
        public IEnumerable<T> GetAll(SimpleArtist artist);
        public IEnumerable<TimelineItem<T>> GetAllTimelineItems(SimpleArtist artist);
    }
}