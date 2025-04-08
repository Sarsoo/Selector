namespace Selector;

public class Listen : IListen
{
    public required string TrackName { get; set; }
    public required string AlbumName { get; set; }
    public required string ArtistName { get; set; }

    public DateTime Timestamp { get; set; }
}