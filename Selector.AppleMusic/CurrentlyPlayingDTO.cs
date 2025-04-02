using Selector.AppleMusic.Model;

namespace Selector.AppleMusic
{
    public class AppleCurrentlyPlayingDTO
    {
        public Track Track { get; set; }

        // public string Username { get; set; }
        public string UserId { get; set; }

        public static explicit operator AppleCurrentlyPlayingDTO(AppleListeningChangeEventArgs e)
        {
            return new()
            {
                Track = e.Current.Track,
                UserId = e.Id
            };
        }

        // public override string ToString() => $"[{Username}] [{Track}]";
        public override string ToString() => $"[{Track}]";
    }
}