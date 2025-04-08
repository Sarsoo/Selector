using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.Mapping
{
    /// <inheritdoc/>
    public class ScrobbleAlbumMapping : ScrobbleMapping
    {
        public string AlbumName { get; set; }
        public string ArtistName { get; set; }

        public ScrobbleAlbumMapping(ISearchClient _searchClient, ILogger<ScrobbleAlbumMapping> _logger,
            string albumName, string artistName) : base(_searchClient, _logger)
        {
            AlbumName = albumName;
            ArtistName = artistName;
        }

        private SimpleAlbum? _result;
        public SimpleAlbum? Album => _result;
        public override object? Result => _result;

        public override string Query => $"{AlbumName} {ArtistName}";

        public override SearchRequest.Types QueryType => SearchRequest.Types.Album;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Albums.Items?.FirstOrDefault();

            if (topResult is not null
                && topResult.Name.Equals(AlbumName, StringComparison.InvariantCultureIgnoreCase)
                && topResult.Artists.First().Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                _result = topResult;
            }
        }
    }
}