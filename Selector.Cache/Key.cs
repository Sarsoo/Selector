using System;
using System.Collections.Generic;
using System.Text;

namespace Selector.Cache
{
    public class Key
    {
        public const string CurrentlyPlayingName = "CurrentlyPlaying";

        public const string TrackName = "Track";
        public const string AlbumName = "Album";
        public const string ArtistName = "Artist";
        public const string UserName = "User";

        public const string AudioFeatureName = "AudioFeature";
        public const string PlayCountName = "PlayCount";

        public const string WorkerName = "Worker";
        public const string WatcherName = "Watcher";
        public const string ReservedName = "Reserved";

        /// <summary>
        /// Current playback for a user
        /// </summary>
        /// <param name="user">User's database Id (Guid)</param>
        /// <returns></returns>
        public static string CurrentlyPlaying(string user) => Namespace(user, CurrentlyPlayingName);
        public static string AudioFeature(string trackId) => Namespace(TrackName, trackId, AudioFeatureName);

        public static string TrackPlayCount(string name, string artist) => Namespace(TrackName, artist, name, PlayCountName);
        public static string AlbumPlayCount(string name, string artist) => Namespace(AlbumName, artist, name, PlayCountName);
        public static string ArtistPlayCount(string name) => Namespace(ArtistName, name, PlayCountName);
        public static string UserPlayCount(string username) => Namespace(UserName, username, PlayCountName);

        public static string WatcherReserved(int id) => Namespace(WatcherName, id.ToString(), ReservedName);

        public static string Namespace(params string[] args) => string.Join(":", args);
    }
}
