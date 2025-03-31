using Selector.Spotify.Timeline;
using SpotifyAPI.Web;

namespace Selector.Spotify
{
    public class SpotifyListeningChangeEventArgs : ListeningChangeEventArgs
    {
        public CurrentlyPlayingContext Previous { get; set; }
        public CurrentlyPlayingContext Current { get; set; }

        /// <summary>
        /// Spotify Username
        /// </summary>
        public string SpotifyUsername { get; set; }

        PlayerTimeline Timeline { get; set; }

        public static SpotifyListeningChangeEventArgs From(CurrentlyPlayingContext previous,
            CurrentlyPlayingContext current, PlayerTimeline timeline, string id = null, string username = null)
        {
            return new SpotifyListeningChangeEventArgs()
            {
                Previous = previous,
                Current = current,
                Timeline = timeline,
                Id = id,
                SpotifyUsername = username
            };
        }
    }

    public class PlaylistChangeEventArgs : EventArgs
    {
        public FullPlaylist Previous { get; set; }
        public FullPlaylist Current { get; set; }

        /// <summary>
        /// Spotify Username
        /// </summary>
        public string SpotifyUsername { get; set; }

        /// <summary>
        /// String Id for watcher, used to hold user Db Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }

        Timeline<FullPlaylist> Timeline { get; set; }
        ICollection<PlaylistTrack<IPlayableItem>> CurrentTracks { get; set; }
        ICollection<PlaylistTrack<IPlayableItem>> AddedTracks { get; set; }
        ICollection<PlaylistTrack<IPlayableItem>> RemovedTracks { get; set; }

        public static PlaylistChangeEventArgs From(FullPlaylist previous, FullPlaylist current,
            Timeline<FullPlaylist> timeline, ICollection<PlaylistTrack<IPlayableItem>> tracks = null,
            ICollection<PlaylistTrack<IPlayableItem>> addedTracks = null,
            ICollection<PlaylistTrack<IPlayableItem>> removedTracks = null, string id = null, string username = null)
        {
            return new PlaylistChangeEventArgs()
            {
                Previous = previous,
                Current = current,
                Timeline = timeline,
                CurrentTracks = tracks,
                AddedTracks = addedTracks,
                RemovedTracks = removedTracks,
                Id = id,
                SpotifyUsername = username
            };
        }
    }
}