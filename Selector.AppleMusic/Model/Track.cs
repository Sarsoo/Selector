namespace Selector.AppleMusic.Model;

public class TrackAttributes
{
    public string AlbumName { get; set; }
    public List<string> GenreNames { get; set; }
    public int TrackNumber { get; set; }
    public int DurationInMillis { get; set; }
    public DateTime ReleaseDate { get; set; }

    public string Isrc { get; set; }

    //TODO: Artwork
    public Artwork Artwork { get; set; }
    public string ComposerName { get; set; }
    public string Url { get; set; }
    public PlayParams PlayParams { get; set; }
    public int DiscNumber { get; set; }
    public bool HasLyrics { get; set; }
    public bool IsAppleDigitalMaster { get; set; }

    public string Name { get; set; }

    //TODO: previews
    public string ArtistName { get; set; }
}

public class Artwork
{
    public string Url { get; set; }
}

public class PlayParams
{
    public string Id { get; set; }
    public string Kind { get; set; }
}

public class Track
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Href { get; set; }
    public TrackAttributes Attributes { get; set; }

    public override string ToString()
    {
        return $"{Attributes?.Name} / {Attributes?.AlbumName} / {Attributes?.ArtistName}";
    }
}