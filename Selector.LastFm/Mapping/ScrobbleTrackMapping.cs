using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Selector.Mapping
{
    /// <inheritdoc/>
    public class ScrobbleTrackMapping : ScrobbleMapping
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }

        public ScrobbleTrackMapping(ISearchClient searchClient, ILogger<ScrobbleTrackMapping> logger,
            string trackName, string artistName) : base(searchClient, logger)
        {
            TrackName = trackName;
            ArtistName = artistName;
        }

        private FullTrack? _result;
        public FullTrack? Track => _result;
        public override object? Result => _result;

        public override string Query => $"{TrackName} {ArtistName}";

        public override SearchRequest.Types QueryType => SearchRequest.Types.Track;

        public override void HandleResponse(Task<SearchResponse> response)
        {
            var topResult = response.Result.Tracks.Items?.FirstOrDefault();

            if (topResult is not null
                && topResult.Name.Equals(TrackName, StringComparison.InvariantCultureIgnoreCase)
                && topResult.Artists.First().Name.Equals(ArtistName, StringComparison.InvariantCultureIgnoreCase))
            {
                _result = topResult;
            }
        }
    }
}