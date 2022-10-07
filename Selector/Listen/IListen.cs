using System;

namespace Selector;

public interface IListen
{
    DateTime Timestamp { get; set; }

    string TrackName { get; set; }
    string AlbumName { get; set; }
    string ArtistName { get; set; }
}

