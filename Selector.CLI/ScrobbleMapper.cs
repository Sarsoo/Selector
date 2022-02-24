using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using Selector.Operations;
using SpotifyAPI.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                logger.LogInformation("Mapping scrobble artists");

                var currentArtists = mappingRepo.GetArtists();
                var scrobbleArtists = scrobbleRepo.GetAll()
                    .GroupBy(x => x.ArtistName)
                    .Select(x => (x.Key, x.Count()))
                    .OrderByDescending(x => x.Item2)
                    .Select(x => x.Key);

                if(config.Limit is not null)
                {
                    scrobbleArtists = scrobbleArtists.Take(config.Limit.Value);
                }

                var artistsToPull = scrobbleArtists
                    .ExceptBy(currentArtists.Select(a => a.LastfmArtistName), a => a);


                var requests = artistsToPull.Select(a => new ScrobbleArtistMapping(
                     searchClient, 
                     loggerFactory.CreateLogger<ScrobbleArtistMapping>() ?? NullLogger<ScrobbleArtistMapping>.Instance,
                     a)
                ).ToList();

                logger.LogInformation("Found {} artists to map, starting", requests.Count);

                var batchRequest = new BatchingOperation<ScrobbleArtistMapping>(
                    config.InterRequestDelay, 
                    config.Timeout, 
                    config.SimultaneousConnections, 
                    requests
                );

                await batchRequest.TriggerRequests(token);

                logger.LogInformation("Finished mapping artists");

                var newArtists = batchRequest.DoneRequests
                    .Where(a => a is not null)
                    .Select(a => (FullArtist) a.Result)
                    .Where(a => a is not null);
                var newMappings = newArtists.Select(a => new ArtistLastfmSpotifyMapping() { 
                    LastfmArtistName = a.Name,
                    SpotifyUri = a.Uri
                });

                mappingRepo.AddRange(newMappings);
            }

            await mappingRepo.Save();
        }
    }
}
