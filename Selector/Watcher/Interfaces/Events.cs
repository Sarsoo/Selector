using System;
using SpotifyAPI.Web;

namespace Selector
{
    public class ListeningChangeEventArgs: EventArgs {
        public CurrentlyPlayingContext Previous { get; set; }
        public CurrentlyPlayingContext Current { get; set; }
        /// <summary>
        /// Spotify Username
        /// </summary>
        public string SpotifyUsername { get; set; }
        /// <summary>
        /// String Id for watcher, used to hold user Db Id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        PlayerTimeline Timeline { get; set; }

        public static ListeningChangeEventArgs From(CurrentlyPlayingContext previous, CurrentlyPlayingContext current, PlayerTimeline timeline, string id = null, string username = null)
        {
            return new ListeningChangeEventArgs()
            {
                Previous = previous,
                Current = current,
                Timeline = timeline,
                Id = id,
                SpotifyUsername = username
            };
        }
    }
}
