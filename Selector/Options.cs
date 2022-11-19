using System;
namespace Selector
{
    public class NowPlayingOptions
    {
        public const string Key = "Now";

        public TimeSpan ArtistResampleWindow { get; set; } = TimeSpan.FromDays(7);
        public TimeSpan AlbumResampleWindow { get; set; } = TimeSpan.FromDays(7);
        public TimeSpan TrackResampleWindow { get; set; } = TimeSpan.FromDays(7);

        public TimeSpan ArtistDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal ArtistDensityThreshold { get; set; } = 5;

        public TimeSpan AlbumDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal AlbumDensityThreshold { get; set; } = 5;

        public TimeSpan TrackDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal TrackDensityThreshold { get; set; } = 5;
    }

    public class PastOptions
    {
        public const string Key = "Past";

        public TimeSpan ResampleWindow { get; set; } = TimeSpan.FromDays(7);
        public int RankingCount { get; set; } = 20;
    }
}

