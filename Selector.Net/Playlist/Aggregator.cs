using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.Net.Playlist
{
    public class AggregatorConfig
    {
        public string PlaylistId { get; set; }
    }

    public class Aggregator<TNodeId>: BaseSingleSink<TNodeId, PlaylistChangeEventArgs>
    {
        private readonly ILogger<Aggregator<TNodeId>> Logger;
        private readonly ISpotifyClient SpotifyClient;
        private readonly ICurrentItemListResolver ItemResolver;
        private readonly AggregatorConfig Config;

        public Aggregator(ISpotifyClient spotifyClient, ICurrentItemListResolver itemResolver, AggregatorConfig config, ILogger<Aggregator<TNodeId>> logger)
        {
            Logger = logger;
            SpotifyClient = spotifyClient;
            ItemResolver = itemResolver;
            Config = config;
        }

        public override async Task ConsumeType(PlaylistChangeEventArgs obj)
        {
            try
            {
                var addedTracks = obj.AddedTracks.ToList();
                var removedTracks = obj.AddedTracks.ToList();
                var removedTrackURIs = obj.AddedTracks.ToList();

                var currentTracks = await ItemResolver.GetCurrentItems().ConfigureAwait(false);

                currentTracks = currentTracks.Where(t => addedTracks);
                currentTracks = currentTracks.Concat(addedTracks);

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

