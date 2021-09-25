using System;
using SpotifyAPI.Web;

namespace Selector
{
    public class ListeningChangeEventArgs: EventArgs {
        public CurrentlyPlaying Previous;
        public CurrentlyPlaying Current;
    }
}
