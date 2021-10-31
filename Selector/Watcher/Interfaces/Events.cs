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
        public string Username { get; set; }
        PlayerTimeline Timeline { get; set; }

        public static ListeningChangeEventArgs From(CurrentlyPlayingContext previous, CurrentlyPlayingContext current, PlayerTimeline timeline, string username = null)
        {
            return new ListeningChangeEventArgs()
            {
                Previous = previous,
                Current = current,
                Timeline = timeline,
                Username = username
            };
        }
    }
}
