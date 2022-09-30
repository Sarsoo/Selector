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
    public class ScrobbleArtistMapping : ScrobbleMapping
    {
        public string ArtistName { get; set; }

        public ScrobbleArtistMapping(ISearchClient _searchClient, ILogger<ScrobbleArtistMapping> _logger, string artistName) : base(_searchClient, _logger)
        {
            ArtistName = artistName;
        }

        private FullArtist result;
        public FullArtist Artist => result;
        public override object Result => result;

        public override string Query => ArtistName;

        public override SearchRequest.Types QueryType => SearchRequest.Types.Artist;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Artists.Items.FirstOrDefault();

            if (topResult is not null
                && topResult.Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                result = topResult;
            }
        }
    }
}
