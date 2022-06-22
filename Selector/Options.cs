using System;
namespace Selector
{
    public class NowPlayingOptions
    {
        public const string Key = "Now";

        public TimeSpan ArtistResampleWindow { get; set; } = TimeSpan.FromDays(30);
        public TimeSpan AlbumResampleWindow { get; set; } = TimeSpan.FromDays(30);
        public TimeSpan TrackResampleWindow { get; set; } = TimeSpan.FromDays(30);

        public TimeSpan ArtistDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal ArtistDensityThreshold { get; set; } = 5;

        public TimeSpan AlbumDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal AlbumDensityThreshold { get; set; } = 5;

        public TimeSpan TrackDensityWindow { get; set; } = TimeSpan.FromDays(10);
        public decimal TrackDensityThreshold { get; set; } = 5;
    }
}

