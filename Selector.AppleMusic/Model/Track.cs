namespace Selector.AppleMusic.Model;

public class TrackAttributes
{
    public required string AlbumName { get; set; }
    public required List<string> GenreNames { get; set; }
    public int TrackNumber { get; set; }
    public int DurationInMillis { get; set; }
    public DateTime ReleaseDate { get; set; }

    public required string Isrc { get; set; }

    //TODO: Artwork
    public required Artwork Artwork { get; set; }
    public string? ComposerName { get; set; }
    public required string Url { get; set; }
    public required PlayParams PlayParams { get; set; }
    public int DiscNumber { get; set; }
    public bool HasLyrics { get; set; }
    public bool IsAppleDigitalMaster { get; set; }

    public required string Name { get; set; }

    //TODO: previews
    public required string ArtistName { get; set; }
}

public class Artwork
{
    public required string Url { get; set; }
}

public class PlayParams
{
    public required string Id { get; set; }
    public required string Kind { get; set; }
}

public class Track
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Href { get; set; }
    public required TrackAttributes Attributes { get; set; }

    public override string ToString()
    {
        return $"{Attributes?.Name} / {Attributes?.AlbumName} / {Attributes?.ArtistName}";
    }
}