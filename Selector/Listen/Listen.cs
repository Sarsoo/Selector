using System;

namespace Selector;

public class Listen: IListen
{
    public string TrackName { get; set; }
    public string AlbumName { get; set; }
    public string ArtistName { get; set; }

    public DateTime Timestamp { get; set; }
}

