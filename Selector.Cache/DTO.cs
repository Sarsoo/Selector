using System;

using SpotifyAPI.Web;

namespace Selector.Cache {

    public class CurrentlyPlayingDTO {
        public CurrentlyPlayingContext Context { get; set; }
        public string Username { get; set; }

        public FullTrack Track { get; set; }
        public FullEpisode Episode { get; set; }

        public static explicit operator CurrentlyPlayingDTO(ListeningChangeEventArgs e)
        {
            if(e.Current.Item is FullTrack track)
            {
                return new()
                {
                    Context = e.Current,
                    Username = e.Username,
                    Track = track
                };
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                return new()
                {
                    Context = e.Current,
                    Username = e.Username,
                    Episode = episode
                };
            }
            else
            {
                throw new ArgumentException("Unknown item item");
            }
        }
    }
}