using System;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist
{
    public class ApplierConfig {
        public string PlaylistId { get; set; }
    }

    public class Applier<TNodeId>: BaseSingleSink<TNodeId, PlaylistChangeEventArgs>
    {
        private readonly ILogger<Applier<TNodeId>> Logger;
        private readonly ISpotifyClient SpotifyClient;
        private readonly ApplierConfig Config;

        private FullPlaylist Playlist { get; set; }
        private IEnumerable<PlaylistTrack<IPlayableItem>> Items { get; set; }

        public Applier(ISpotifyClient spotifyClient, ApplierConfig config, ILogger<Applier<TNodeId>> logger)
        {
            Logger = logger;
            SpotifyClient = spotifyClient;
            Config = config;
        }

        public override async Task ConsumeType(PlaylistChangeEventArgs obj)
        {
            try
            {
                var tracks = obj.CurrentTracks.ToList();
                var trackUris = tracks.Select(t => t.GetUri()).ToList();

                await SpotifyClient.Playlists.ReplaceItems(Config.PlaylistId, new PlaylistReplaceItemsRequest(trackUris));
            }
            catch (APIUnauthorizedException e)
            {
                Logger.LogDebug("Unauthorised error: [{message}] (should be refreshed and retried?)", e.Message);
                //throw e;
            }
            catch (APITooManyRequestsException e)
            {
                Logger.LogDebug("Too many requests error: [{message}]", e.Message);
                // throw e;
            }
            catch (APIException e)
            {
                Logger.LogDebug("API error: [{message}]", e.Message);
                // throw e;
            }
        }
    }
}

