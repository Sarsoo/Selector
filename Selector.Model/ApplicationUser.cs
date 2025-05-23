using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Selector.Model
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData] public bool SpotifyIsLinked { get; set; }
        [PersonalData] public DateTime SpotifyLastRefresh { get; set; }
        public int SpotifyTokenExpiry { get; set; }
        public string SpotifyAccessToken { get; set; }
        public string SpotifyRefreshToken { get; set; }

        [PersonalData] public bool AppleMusicLinked { get; set; }
        public string AppleMusicKey { get; set; }
        [PersonalData] public DateTime AppleMusicLastRefresh { get; set; }

        [PersonalData] public string LastFmUsername { get; set; }
        [PersonalData] public string LastFmPassword { get; set; }
        [PersonalData] public bool SaveScrobbles { get; set; }

        public List<Watcher> Watchers { get; set; }
        public List<UserScrobble> Scrobbles { get; set; }
    }

    public class ApplicationUserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool LockoutEnabled { get; set; }

        public bool SpotifyIsLinked { get; set; }
        public DateTime SpotifyLastRefresh { get; set; }
        public int SpotifyTokenExpiry { get; set; }
        public string SpotifyAccessToken { get; set; }
        public string SpotifyRefreshToken { get; set; }

        public bool AppleMusicLinked { get; set; }
        public string AppleMusicKey { get; set; }
        public DateTime AppleMusicLastRefresh { get; set; }

        public string LastFmUsername { get; set; }

        public static explicit operator ApplicationUserDTO(ApplicationUser user) => new()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            LockoutEnabled = user.LockoutEnabled,

            SpotifyIsLinked = user.SpotifyIsLinked,
            SpotifyLastRefresh = user.SpotifyLastRefresh,
            SpotifyTokenExpiry = user.SpotifyTokenExpiry,
            SpotifyAccessToken = user.SpotifyAccessToken,
            SpotifyRefreshToken = user.SpotifyRefreshToken,

            AppleMusicLinked = user.AppleMusicLinked,
            AppleMusicKey = user.AppleMusicKey,
            AppleMusicLastRefresh = user.AppleMusicLastRefresh,

            LastFmUsername = user.LastFmUsername
        };
    }
}