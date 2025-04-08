using Selector.Spotify.Timeline;
using SpotifyAPI.Web;

namespace Selector.Spotify
{
    public class SpotifyListeningChangeEventArgs : ListeningChangeEventArgs
    {
        public CurrentlyPlayingContext? Previous { get; set; }
        public required CurrentlyPlayingContext Current { get; set; }

        /// <summary>
        /// Spotify Username
        /// </summary>
        public required string SpotifyUsername { get; set; }

        public required PlayerTimeline Timeline { get; set; }

        public static SpotifyListeningChangeEventArgs From(CurrentlyPlayingContext? previous,
            CurrentlyPlayingContext current, PlayerTimeline timeline, string id, string username)
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
        public FullPlaylist? Previous { get; set; }
        public required FullPlaylist Current { get; set; }

        /// <summary>
        /// Spotify Username
        /// </summary>
        public required string SpotifyUsername { get; set; }

        /// <summary>
        /// String Id for watcher, used to hold user Db Id
        /// </summary>
        /// <value></value>
        public required string Id { get; set; }

        public required Timeline<FullPlaylist> Timeline { get; set; }
        private ICollection<PlaylistTrack<IPlayableItem>>? CurrentTracks { get; set; }
        private ICollection<PlaylistTrack<IPlayableItem>>? AddedTracks { get; set; }
        private ICollection<PlaylistTrack<IPlayableItem>>? RemovedTracks { get; set; }

        public static PlaylistChangeEventArgs From(FullPlaylist previous, FullPlaylist current,
            Timeline<FullPlaylist> timeline, string id, string username,
            ICollection<PlaylistTrack<IPlayableItem>>? tracks = null,
            ICollection<PlaylistTrack<IPlayableItem>>? addedTracks = null,
            ICollection<PlaylistTrack<IPlayableItem>>? removedTracks = null)
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