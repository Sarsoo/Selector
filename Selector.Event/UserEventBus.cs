using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.Model;
using Selector.Spotify;

namespace Selector.Events
{
    public class UserEventBus : IEventBus
    {
        private readonly ILogger<UserEventBus> Logger;

        public event EventHandler<ApplicationUser> UserChange;
        public event EventHandler<SpotifyLinkChange> SpotifyLinkChange;
        public event EventHandler<AppleMusicLinkChange> AppleLinkChange;
        public event EventHandler<LastfmChange> LastfmCredChange;

        public event EventHandler<SpotifyCurrentlyPlayingDTO> CurrentlyPlayingSpotify;
        public event EventHandler<AppleCurrentlyPlayingDTO> CurrentlyPlayingApple;

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

        public void OnAppleMusicLinkChange(object sender, AppleMusicLinkChange args)
        {
            Logger.LogTrace("Firing user Apple Music event [{usernamne}]", args?.UserId);
            AppleLinkChange?.Invoke(sender, args);
        }

        public void OnLastfmCredChange(object sender, LastfmChange args)
        {
            Logger.LogTrace("Firing user Last.fm event [{usernamne}]", args?.UserId);
            LastfmCredChange?.Invoke(sender, args);
        }

        public void OnCurrentlyPlayingChangeSpotify(object sender, SpotifyCurrentlyPlayingDTO args)
        {
            Logger.LogTrace("Firing currently playing event [{usernamne}/{userId}]", args?.Username, args.UserId);
            CurrentlyPlayingSpotify?.Invoke(sender, args);
        }

        public void OnCurrentlyPlayingChangeApple(object sender, AppleCurrentlyPlayingDTO args)
        {
            Logger.LogTrace("Firing currently playing event");
            CurrentlyPlayingApple?.Invoke(sender, args);
        }
    }
}