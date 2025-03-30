using System.Linq;

namespace Selector.Cache
{
    public class Key
    {
        public const char MajorSep = ':';
        public const char MinorSep = '.';

        public const string All = "*";
        public const string CurrentlyPlayingName = "CURRENTLY_PLAYING";

        public const string TrackName = "TRACK";
        public const string AlbumName = "ALBUM";
        public const string ArtistName = "ARTIST";
        public const string UserName = "USER";

        public const string AudioFeatureName = "AUDIO_FEATURE";
        public const string PlayCountName = "PLAY_COUNT";
        public const string Duration = "DURATION";

        public const string SpotifyName = "SPOTIFY";
        public const string AppleMusicName = "APPLEMUSIC";
        public const string LastfmName = "LASTFM";

        public const string WatcherName = "WATCHER";

        /// <summary>
        /// Current playback for a user
        /// </summary>
        /// <param name="user">User's database Id (Guid)</param>
        /// <returns></returns>
        public static string CurrentlyPlayingSpotify(string user) =>
            MajorNamespace(MinorNamespace(UserName, SpotifyName, CurrentlyPlayingName), user);

        public static string CurrentlyPlayingAppleMusic(string user) =>
            MajorNamespace(MinorNamespace(UserName, AppleMusicName, CurrentlyPlayingName), user);

        public static readonly string AllCurrentlyPlayingSpotify = CurrentlyPlayingSpotify(All);
        public static readonly string AllCurrentlyPlayingApple = CurrentlyPlayingAppleMusic(All);

        public static string Track(string trackId) => MajorNamespace(TrackName, trackId);
        public static readonly string AllTracks = Track(All);

        public static string AudioFeature(string trackId) =>
            MajorNamespace(MinorNamespace(TrackName, AudioFeatureName), trackId);

        public static readonly string AllAudioFeatures = AudioFeature(All);

        public static string TrackPlayCount(string username, string name, string artist) =>
            MajorNamespace(MinorNamespace(TrackName, PlayCountName), artist, name, username);

        public static string AlbumPlayCount(string username, string name, string artist) =>
            MajorNamespace(MinorNamespace(AlbumName, PlayCountName), artist, name, username);

        public static string ArtistPlayCount(string username, string name) =>
            MajorNamespace(MinorNamespace(ArtistName, PlayCountName), name, username);

        public static string UserPlayCount(string username) =>
            MajorNamespace(MinorNamespace(UserName, PlayCountName), username);

        public static string UserSpotify(string username) =>
            MajorNamespace(MinorNamespace(UserName, SpotifyName), username);

        public static string UserAppleMusic(string username) =>
            MajorNamespace(MinorNamespace(UserName, AppleMusicName), username);

        public static readonly string AllUserSpotify = UserSpotify(All);
        public static readonly string AllUserAppleMusic = UserAppleMusic(All);

        public static string UserLastfm(string username) =>
            MajorNamespace(MinorNamespace(UserName, LastfmName), username);

        public static readonly string AllUserLastfm = UserLastfm(All);

        public static string Watcher(int id) => MajorNamespace(WatcherName, id.ToString());
        public static readonly string AllWatcher = MajorNamespace(WatcherName, All);

        public static string MajorNamespace(params string[] args) => Namespace(MajorSep, args);
        public static string MinorNamespace(params string[] args) => Namespace(MinorSep, args);
        public static string Namespace(char separator, params string[] args) => string.Join(separator, args);

        public static string[] UnMajorNamespace(string arg) => UnNamespace(arg, MajorSep);
        public static string[] UnMinorNamespace(string arg) => UnNamespace(arg, MinorSep);
        public static string[] UnNamespace(string key, params char[] args) => key.Split(args);

        public static string Param(string key) => UnMajorNamespace(key).Skip(1).First();

        public static (string, string) ParamPair(string key)
        {
            var split = UnMajorNamespace(key);
            return (split[1], split[2]);
        }

        public static (string, string, string) ParamTriplet(string key)
        {
            var split = UnMajorNamespace(key);
            return (split[1], split[2], split[3]);
        }
    }
}