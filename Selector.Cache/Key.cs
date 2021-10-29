using System;
using System.Collections.Generic;
using System.Text;

namespace Selector.Cache
{
    public class Key
    {
        public const string CurrentlyPlayingName = "CurrentlyPlaying";
        public const string TrackName = "Track";
        public const string AudioFeatureName = "AudioFeature";

        public static string CurrentlyPlaying(string user) => Namespace(new[] { user, CurrentlyPlayingName });
        public static string AudioFeature(string trackId) => Namespace(new[] { TrackName, trackId, AudioFeatureName });

        public static string Namespace(string[] args) => string.Join(":", args);
    }
}
