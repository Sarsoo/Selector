using Microsoft.Extensions.Logging;

using Selector.Model;

namespace Selector.Events
{
    public class UserEventBus: IEventBus
    {
        private readonly ILogger<UserEventBus> Logger;

        public event EventHandler<ApplicationUser> UserChange;
        public event EventHandler<SpotifyLinkChange> SpotifyLinkChange;
        public event EventHandler<LastfmChange> LastfmCredChange;

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

        public void OnSpotifyLinkChange(object sender, SpotifyLinkChange args)
        {
            Logger.LogTrace("Firing user Spotify event [{usernamne}]", args?.UserId);
            SpotifyLinkChange?.Invoke(sender, args);
        }

        public void OnLastfmCredChange(object sender, LastfmChange args)
        {
            Logger.LogTrace("Firing user Last.fm event [{usernamne}]", args?.UserId);
            LastfmCredChange?.Invoke(sender, args);
        }

        public void OnCurrentlyPlayingChange(object sender, string userId, CurrentlyPlayingDTO args)
        {
            Logger.LogTrace("Firing currently playing event [{usernamne}/{userId}]", args?.Username, userId);
            CurrentlyPlaying?.Invoke(sender, (userId, args));
        }
    }
}
