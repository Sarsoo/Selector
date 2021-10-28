using System;
using SpotifyAPI.Web;

namespace Selector
{
    public class ListeningChangeEventArgs: EventArgs {
        public CurrentlyPlayingContext Previous;
        public CurrentlyPlayingContext Current;
        public string Username;

        public static ListeningChangeEventArgs From(CurrentlyPlayingContext previous, CurrentlyPlayingContext current, string username = null)
        {
            return new ListeningChangeEventArgs()
            {
                Previous = previous,
                Current = current,
                Username = username
            };
        }
    }
}
