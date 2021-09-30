using System;
using SpotifyAPI.Web;

namespace Selector
{
    public class ListeningChangeEventArgs: EventArgs {
        public CurrentlyPlayingContext Previous;
        public CurrentlyPlayingContext Current;

        public static ListeningChangeEventArgs From(CurrentlyPlayingContext previous, CurrentlyPlayingContext current)
        {
            return new ListeningChangeEventArgs()
            {
                Previous = previous,
                Current = current
            };
        }
    }
}
