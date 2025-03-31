using Selector.Extensions;
using SpotifyAPI.Web;

namespace Selector.Spotify
{
    public class CurrentlyPlayingDTO
    {
        public CurrentlyPlayingContextDTO Context { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }

        public FullTrack Track { get; set; }
        public FullEpisode Episode { get; set; }

        public static explicit operator CurrentlyPlayingDTO(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current.Item is FullTrack track)
            {
                return new()
                {
                    Context = e.Current,
                    Username = e.SpotifyUsername,
                    Track = track,
                    UserId = e.Id
                };
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                return new()
                {
                    Context = e.Current,
                    Username = e.SpotifyUsername,
                    Episode = episode,
                    UserId = e.Id
                };
            }
            else
            {
                throw new ArgumentException("Unknown item item");
            }
        }

        public override string ToString() => $"[{Username}] [{Context}]";
    }

    public class CurrentlyPlayingContextDTO
    {
        public Device Device { get; set; }
        public string RepeatState { get; set; }
        public bool ShuffleState { get; set; }
        public Context Context { get; set; }
        public long Timestamp { get; set; }
        public int ProgressMs { get; set; }
        public bool IsPlaying { get; set; }

        public string CurrentlyPlayingType { get; set; }
        public Actions Actions { get; set; }

        public static implicit operator CurrentlyPlayingContextDTO(CurrentlyPlayingContext context)
        {
            return new CurrentlyPlayingContextDTO
            {
                Device = context.Device,
                RepeatState = context.RepeatState,
                ShuffleState = context.ShuffleState,
                Context = context.Context,
                Timestamp = context.Timestamp,
                ProgressMs = context.ProgressMs,
                IsPlaying = context.IsPlaying,
                CurrentlyPlayingType = context.CurrentlyPlayingType,
                Actions = context.Actions
            };
        }

        public override string ToString() => $"{IsPlaying}, {Device?.DisplayString()}";
    }
}