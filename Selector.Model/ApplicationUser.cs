using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Selector.Model
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public bool SpotifyIsLinked { get; set; }
        public DateTime SpotifyLastRefresh { get; set; }
        public int SpotifyTokenExpiry { get; set; }
        public string SpotifyAccessToken { get; set; }
        public string SpotifyRefreshToken { get; set; }

        public string LastFmUsername { get; set; }

        public List<Watcher> Watchers { get; set; }
    }
}