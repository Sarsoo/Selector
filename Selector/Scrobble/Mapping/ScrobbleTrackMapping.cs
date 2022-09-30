using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector
{
    /// <inheritdoc/>
    public class ScrobbleTrackMapping : ScrobbleMapping
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }

        public ScrobbleTrackMapping(ISearchClient _searchClient, ILogger<ScrobbleTrackMapping> _logger, string trackName, string artistName) : base(_searchClient, _logger)
        {
            TrackName = trackName;
            ArtistName = artistName;
        }

        private FullTrack result;
        public FullTrack Track => result;
        public override object Result => result;

        public override string Query => $"{TrackName} {ArtistName}";

        public override SearchRequest.Types QueryType => SearchRequest.Types.Track;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Tracks.Items.FirstOrDefault();

            if(topResult is not null 
                && topResult.Name.Equals(TrackName, StringComparison.InvariantCultureIgnoreCase) 
                && topResult.Artists.First().Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                result = topResult;
            }
        }
    }
}
