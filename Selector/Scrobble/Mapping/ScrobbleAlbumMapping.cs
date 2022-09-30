using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Selector
{
    /// <inheritdoc/>
    public class ScrobbleAlbumMapping : ScrobbleMapping
    {
        public string AlbumName { get; set; }
        public string ArtistName { get; set; }

        public ScrobbleAlbumMapping(ISearchClient _searchClient, ILogger<ScrobbleAlbumMapping> _logger, string albumName, string artistName) : base(_searchClient, _logger)
        {
            AlbumName = albumName;
            ArtistName = artistName;
        }

        private SimpleAlbum result;
        public SimpleAlbum Album => result;
        public override object Result => result;

        public override string Query => $"{AlbumName} {ArtistName}";

        public override SearchRequest.Types QueryType => SearchRequest.Types.Album;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Albums.Items.FirstOrDefault();

            if (topResult is not null
                && topResult.Name.Equals(AlbumName, StringComparison.InvariantCultureIgnoreCase)
                && topResult.Artists.First().Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                result = topResult;
            }
        }
    }
}
