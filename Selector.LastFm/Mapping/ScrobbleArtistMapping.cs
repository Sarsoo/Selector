using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.Mapping
{
    /// <inheritdoc/>
    public class ScrobbleArtistMapping : ScrobbleMapping
    {
        public string ArtistName { get; set; }

        public ScrobbleArtistMapping(ISearchClient searchClient, ILogger<ScrobbleArtistMapping> logger,
            string artistName) : base(searchClient, logger)
        {
            ArtistName = artistName;
        }

        private FullArtist? _result;
        public FullArtist? Artist => _result;
        public override object? Result => _result;

        public override string Query => ArtistName;

        public override SearchRequest.Types QueryType => SearchRequest.Types.Artist;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Artists.Items?.FirstOrDefault();

            if (topResult is not null
                && topResult.Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                _result = topResult;
            }
        }
    }
}