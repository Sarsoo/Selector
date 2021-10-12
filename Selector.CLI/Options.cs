using System.Collections.Generic;

namespace Selector.CLI
{
    class RootOptions
    {
        public const string Key = "Selector";

        /// <summary>
        /// Spotify client ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Spotify app secret
        /// </summary>
        public string ClientSecret { get; set; }
        public WatcherOptions WatcherOptions { get; set; } = new();
        public EqualityChecker Equality { get; set; } = EqualityChecker.Uri;
    }

    enum EqualityChecker
    {
        Uri, String
    }

    class WatcherOptions
    {
        public const string Key = "Watcher";

        public List<WatcherInstanceOptions> Instances { get; set; } = new();
    }

    class WatcherInstanceOptions
    {
        public const string Key = "Instances";

        public string Name { get; set; }
        public string AccessKey { get; set; }
        public string RefreshKey { get; set; }
        public int PollPeriod { get; set; } = 5000;
        public WatcherType Type { get; set; } = WatcherType.Player;
#nullable enable
        public string? PlaylistUri { get; set; }
        public string? WatcherCollection { get; set; }
#nullable disable
    }

    enum WatcherType
    {
        Player, Playlist
    }
}
