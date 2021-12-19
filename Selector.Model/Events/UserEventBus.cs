using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Events;

namespace Selector.Model.Events
{
    public class UserEventBus: IEventBus
    {
        private readonly ILogger<UserEventBus> Logger;

        public event EventHandler<ApplicationUser> UserChange;
        public event EventHandler<ApplicationUser> SpotifyLinkChange;
        public event EventHandler<ApplicationUser> LastfmCredChange;

        public event EventHandler<(string, CurrentlyPlayingDTO)> CurrentlyPlaying;

        public UserEventBus(ILogger<UserEventBus> logger)
        {
            Logger = logger;
        }

        public void OnUserChange(object sender, ApplicationUser args)
        {
            Logger.LogTrace("Firing user event [{usernamne}]", args?.UserName);
            UserChange?.Invoke(sender, args);
        }

        public void OnSpotifyLinkChange(object sender, ApplicationUser args)
        {
            Logger.LogTrace("Firing user Spotify event [{usernamne}]", args?.UserName);
            SpotifyLinkChange?.Invoke(sender, args);
        }

        public void OnLastfmCredChange(object sender, ApplicationUser args)
        {
            Logger.LogTrace("Firing user Last.fm event [{usernamne}]", args?.UserName);
            LastfmCredChange?.Invoke(sender, args);
        }

        public void OnCurrentlyPlayingChange(object sender, string userId, CurrentlyPlayingDTO args)
        {
            Logger.LogTrace("Firing currently playing event [{usernamne}/{userId}]", args?.Username, userId);
            CurrentlyPlaying?.Invoke(sender, (userId, args));
        }
    }
}
