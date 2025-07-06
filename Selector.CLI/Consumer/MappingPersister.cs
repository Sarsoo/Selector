using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Selector.Extensions;
using Selector.Model;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using SpotifyAPI.Web;

namespace Selector.CLI.Consumer
{
    /// <summary>
    /// Save name -> Spotify URI mappings as new objects come through the watcher without making extra queries of the Spotify API
    /// </summary>
    public class MappingPersister : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>,
        ISpotifyPlayerConsumer
    {
        protected readonly IServiceScopeFactory ScopeFactory;

        public CancellationToken CancelToken { get; set; }

        public MappingPersister(
            ISpotifyPlayerWatcher watcher,
            IServiceScopeFactory scopeFactory,
            ILogger<MappingPersister> logger = null,
            CancellationToken token = default
        ) : base(watcher, logger)
        {
            ScopeFactory = scopeFactory;
        }

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;
            using var span = Trace.Tracer.StartActivity();
            span?.AddBaggage(TraceConst.SpotifyUsername, e.SpotifyUsername);
            span?.AddBaggage(TraceConst.UserId, e.Id);

            using var serviceScope = ScopeFactory.CreateScope();
            using var scope = Logger.BeginScope(new Dictionary<string, object>()
                { { TraceConst.SpotifyUsername, e.SpotifyUsername }, { TraceConst.UserId, e.Id } });

            if (e.Current.Item is FullTrack track)
            {
                span?.AddBaggage(TraceConst.TrackName, track.Name);
                span?.AddBaggage(TraceConst.AlbumName, track.Album.Name);
                span?.AddBaggage(TraceConst.ArtistName, track.Artists.FirstOrDefault()?.Name);

                var mappingRepo = serviceScope.ServiceProvider.GetRequiredService<IScrobbleMappingRepository>();

                if (!mappingRepo.GetTracks().Select(t => t.SpotifyUri).Contains(track.Uri))
                {
                    span?.AddEvent(new ActivityEvent("Adding Track Mapping"));
                    mappingRepo.Add(new TrackLastfmSpotifyMapping()
                    {
                        SpotifyUri = track.Uri,
                        LastfmTrackName = track.Name,
                        LastfmArtistName = track.Artists.FirstOrDefault()?.Name
                    });
                }

                if (!mappingRepo.GetAlbums().Select(t => t.SpotifyUri).Contains(track.Album.Uri))
                {
                    span?.AddEvent(new ActivityEvent("Adding Album Mapping"));
                    mappingRepo.Add(new AlbumLastfmSpotifyMapping()
                    {
                        SpotifyUri = track.Album.Uri,
                        LastfmAlbumName = track.Album.Name,
                        LastfmArtistName = track.Album.Artists.FirstOrDefault()?.Name
                    });
                }

                var artistUris = mappingRepo.GetArtists().Select(t => t.SpotifyUri).ToArray();
                foreach (var artist in track.Artists)
                {
                    if (!artistUris.Contains(artist.Uri))
                    {
                        span?.AddEvent(new ActivityEvent($"Adding Artist Mapping [{artist.Name}]"));
                        mappingRepo.Add(new ArtistLastfmSpotifyMapping()
                        {
                            SpotifyUri = artist.Uri,
                            LastfmArtistName = artist.Name
                        });
                    }
                }

                await mappingRepo.Save();

                Logger.LogDebug("Adding Spotify <-> Last.fm mapping [{username}]", e.SpotifyUsername);
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                Logger.LogDebug("Ignoring podcast episode [{episode}]", episode.DisplayString());
            }
            else if (e.Current.Item is null)
            {
                Logger.LogDebug("Skipping play count pulling for null item [{context}]", e.Current.DisplayString());
            }
            else
            {
                span?.SetStatus(ActivityStatusCode.Error, "Unknown item pulled from API");
                Logger.LogError("Unknown item pulled from API [{item}]", e.Current.Item);
            }
        }
    }
}