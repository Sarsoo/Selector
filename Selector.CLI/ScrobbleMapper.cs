using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using Selector.Operations;
using SpotifyAPI.Web;
using System;
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
        public bool Tracks { get; set; } = false;
        public bool Albums { get; set; } = false;
        public bool Artists { get; set; } = false;
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
            if (config.Artists)
            {
                await MapArtists(token);
            }

            if (config.Albums)
            {
                await MapAlbums(token);
            }

            if (config.Tracks)
            {
                await MapTracks(token);
            }

            await mappingRepo.Save();
        }

        private async Task MapArtists(CancellationToken token)
        {
            logger.LogInformation("Mapping scrobble artists");

            var currentArtists = mappingRepo.GetArtists();
            var scrobbleArtists = scrobbleRepo.GetAll()
                .GroupBy(x => x.ArtistName)
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Key);

            var artistsToPull = scrobbleArtists
                .ExceptBy(currentArtists.Select(a => a.LastfmArtistName), a => a);

            if (config.Limit is not null)
            {
                artistsToPull = artistsToPull.Take(config.Limit.Value);
            }

            var requests = artistsToPull.Select(a => new ScrobbleArtistMapping(
                 searchClient,
                 loggerFactory.CreateLogger<ScrobbleArtistMapping>() ?? NullLogger<ScrobbleArtistMapping>.Instance,
                 a)
            ).ToArray();

            logger.LogInformation("Found {} artists to map, starting", requests.Length);

            var batchRequest = new BatchingOperation<ScrobbleArtistMapping>(
                config.InterRequestDelay,
                config.Timeout,
                config.SimultaneousConnections,
                requests
            );

            await batchRequest.TriggerRequests(token);

            logger.LogInformation("Finished mapping artists");

            var newArtists = batchRequest.DoneRequests
                .Select(a => a.Result)
                .Cast<FullArtist>()
                .Where(a => a is not null);
            var newMappings = newArtists.Select(a => new ArtistLastfmSpotifyMapping()
            {
                LastfmArtistName = a.Name,
                SpotifyUri = a.Uri
            });

            mappingRepo.AddRange(newMappings);
        }

        private async Task MapAlbums(CancellationToken token)
        {
            logger.LogInformation("Mapping scrobble albums");

            var currentAlbums = mappingRepo.GetAlbums();
            var scrobbleAlbums = scrobbleRepo.GetAll()
                .GroupBy(x => (x.ArtistName, x.AlbumName))
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Key);

            var albumsToPull = scrobbleAlbums
                .ExceptBy(currentAlbums.Select(a => (a.LastfmArtistName, a.LastfmAlbumName)), a => a);

            if (config.Limit is not null)
            {
                albumsToPull = albumsToPull.Take(config.Limit.Value);
            }

            var requests = albumsToPull.Select(a => new ScrobbleAlbumMapping(
                 searchClient,
                 loggerFactory.CreateLogger<ScrobbleAlbumMapping>() ?? NullLogger<ScrobbleAlbumMapping>.Instance,
                 a.AlbumName, a.ArtistName)
            ).ToArray();

            logger.LogInformation("Found {} albums to map, starting", requests.Length);

            var batchRequest = new BatchingOperation<ScrobbleAlbumMapping>(
                config.InterRequestDelay,
                config.Timeout,
                config.SimultaneousConnections,
                requests
            );

            await batchRequest.TriggerRequests(token);

            logger.LogInformation("Finished mapping albums");

            var newArtists = batchRequest.DoneRequests
                .Select(a => a.Result)
                .Cast<SimpleAlbum>()
                .Where(a => a is not null);
            var newMappings = newArtists.Select(a => new AlbumLastfmSpotifyMapping()
            {
                LastfmAlbumName = a.Name,
                LastfmArtistName = a.Artists.FirstOrDefault()?.Name,
                SpotifyUri = a.Uri
            });

            mappingRepo.AddRange(newMappings);
        }

        private async Task MapTracks(CancellationToken token)
        {
            logger.LogInformation("Mapping scrobble tracks");

            var currentTracks = mappingRepo.GetTracks();
            var scrobbleTracks = scrobbleRepo.GetAll()
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

            var batchRequest = new BatchingOperation<ScrobbleTrackMapping>(
                config.InterRequestDelay,
                config.Timeout,
                config.SimultaneousConnections,
                requests
            );

            await batchRequest.TriggerRequests(token);

            logger.LogInformation("Finished mapping tracks");

            var newArtists = batchRequest.DoneRequests
                .Select(a => a.Result)
                .Cast<FullTrack>()
                .Where(a => a is not null);
            var newMappings = newArtists.Select(a => new TrackLastfmSpotifyMapping()
            {
                LastfmTrackName = a.Name,
                LastfmArtistName = a.Artists.FirstOrDefault()?.Name,
                SpotifyUri = a.Uri
            });

            mappingRepo.AddRange(newMappings);
        }
    }
}
