using System;

namespace Selector.Model;

public class SpotifyListen: Listen, IUserListen
{
    public int Id { get; set; }

    public int? PlayedDuration { get; set; }

    public string TrackUri { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}

