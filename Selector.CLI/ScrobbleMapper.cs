using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using Selector.Operations;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Selector
{
    public class ScrobbleMapperConfig
    {
        public TimeSpan InterRequestDelay { get; set; }
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 20, 0);
        public int SimultaneousConnections { get; set; } = 3;
        public int? Limit { get; set; } = null;
    }

    public class ScrobbleMapper
    {
        private readonly ILogger<ScrobbleMapper> logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly ISearchClient searchClient;

        private readonly ScrobbleMapperConfig config;

        private readonly IScrobbleRepository scrobbleRepo;
        private readonly IScrobbleMappingRepository mappingRepo;

        public ScrobbleMapper(ISearchClient _searchClient, ScrobbleMapperConfig _config, IScrobbleRepository _scrobbleRepository, IScrobbleMappingRepository _scrobbleMappingRepository, ILogger<ScrobbleMapper> _logger, ILoggerFactory _loggerFactory = null)
        {
            searchClient = _searchClient;
            config = _config;
            scrobbleRepo = _scrobbleRepository;
            mappingRepo = _scrobbleMappingRepository;
            logger = _logger;
            loggerFactory = _loggerFactory;
        }

        public async Task Execute(CancellationToken token)
        {
            await MapTracks(token);

            await mappingRepo.Save();
        }

        private async Task MapTracks(CancellationToken token)
        {
            logger.LogInformation("Mapping scrobble tracks");

            var currentTracks = mappingRepo.GetTracks().AsEnumerable();
            var scrobbleTracks = scrobbleRepo.GetAll()
                .AsEnumerable()
                .GroupBy(x => (x.ArtistName, x.TrackName))
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Key);

            if (config.Limit is not null)
            {
                scrobbleTracks = scrobbleTracks.Take(config.Limit.Value);
            }

            var tracksToPull = scrobbleTracks
                .ExceptBy(currentTracks.Select(a => (a.LastfmArtistName, a.LastfmTrackName)), a => a);

            var requests = tracksToPull.Select(a => new ScrobbleTrackMapping(
                 searchClient,
                 loggerFactory.CreateLogger<ScrobbleTrackMapping>() ?? NullLogger<ScrobbleTrackMapping>.Instance,
                 a.TrackName, a.ArtistName)
            ).ToArray();

            logger.LogInformation("Found {} tracks to map, starting", requests.Length);

            var batchRequest = GetOperation(requests);

            await batchRequest.TriggerRequests(token);

            logger.LogInformation("Finished mapping tracks");

            var newTracks = batchRequest.DoneRequests
                .Select(a => a.Result)
                .Cast<FullTrack>()
                .Where(a => a is not null);

            var existingTrackUris = currentTracks.Select(a => a.SpotifyUri).ToArray();
            var existingAlbumUris = mappingRepo.GetAlbums().Select(a => a.SpotifyUri).ToArray();
            var existingArtistUris = mappingRepo.GetArtists().Select(a => a.SpotifyUri).ToArray();

            foreach (var track in newTracks)
            {
                if (existingTrackUris.Contains(track.Uri))
                {
                    var artistName = track.Artists.FirstOrDefault()?.Name;
                    var duplicates = currentTracks.Where(a => a.LastfmArtistName.Equals(artistName, StringComparison.OrdinalIgnoreCase)
                                                                && a.LastfmTrackName.Equals(track.Name, StringComparison.OrdinalIgnoreCase));
                    logger.LogWarning("Found duplicate Spotify uri ({}), [{}, {}] {}", 
                        track.Uri, 
                        track.Name, 
                        artistName,
                        string.Join(", ", duplicates.Select(d => $"{d.LastfmTrackName} {d.LastfmArtistName}"))
                    );
                }
                else
                {
                    mappingRepo.Add(new TrackLastfmSpotifyMapping()
                    {
                        LastfmTrackName = track.Name,
                        LastfmArtistName = track.Artists.FirstOrDefault()?.Name,
                        SpotifyUri = track.Uri
                    });
                }

                if(!existingAlbumUris.Contains(track.Album.Uri))
                {
                    mappingRepo.Add(new AlbumLastfmSpotifyMapping()
                    {
                        LastfmAlbumName = track.Album.Name,
                        LastfmArtistName = track.Album.Artists.FirstOrDefault()?.Name,
                        SpotifyUri = track.Album.Uri
                    });
                }

                foreach(var artist in track.Artists.UnionBy(track.Album.Artists, a => a.Name))
                {
                    if (!existingArtistUris.Contains(artist.Uri))
                    {
                        mappingRepo.Add(new ArtistLastfmSpotifyMapping()
                        {
                            LastfmArtistName = artist.Name,
                            SpotifyUri = artist.Uri
                        });
                    }
                }
            }
        }

        private BatchingOperation<T> GetOperation<T>(IEnumerable<T> requests) where T: IOperation 
            => new (config.InterRequestDelay, config.Timeout, config.SimultaneousConnections, requests);
    }
}
