using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Model;
using SpotifyAPI.Web;

namespace Selector.CLI.Consumer
{
    /// <summary>
    /// Save name -> Spotify URI mappings as new objects come through the watcher without making extra queries of the Spotify API
    /// </summary>
    public class MappingPersister: IPlayerConsumer
    {
        protected readonly IPlayerWatcher Watcher;
        protected readonly IServiceScopeFactory ScopeFactory;
        protected readonly ILogger<MappingPersister> Logger;

        public CancellationToken CancelToken { get; set; }

        public MappingPersister(
            IPlayerWatcher watcher,
            IServiceScopeFactory scopeFactory,
            ILogger<MappingPersister> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            ScopeFactory = scopeFactory;
            Logger = logger ?? NullLogger<MappingPersister>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            Task.Run(async () => {
                try
                {
                    await AsyncCallback(e);
                }
                catch (DbUpdateException)
                {
                    Logger.LogWarning("Failed to update database, likely a duplicate Spotify URI");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            using var serviceScope = ScopeFactory.CreateScope();
            using var scope = Logger.BeginScope(new Dictionary<string, object>() { { "spotify_username", e.SpotifyUsername }, { "id", e.Id } });

            if (e.Current.Item is FullTrack track)
            { 
                var mappingRepo = serviceScope.ServiceProvider.GetRequiredService<IScrobbleMappingRepository>();

                if(!mappingRepo.GetTracks().Select(t => t.SpotifyUri).Contains(track.Uri))
                {
                    mappingRepo.Add(new TrackLastfmSpotifyMapping()
                    {
                        SpotifyUri = track.Uri,
                        LastfmTrackName = track.Name,
                        LastfmArtistName = track.Artists.FirstOrDefault()?.Name
                    });
                }

                if (!mappingRepo.GetAlbums().Select(t => t.SpotifyUri).Contains(track.Album.Uri))
                {
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
                Logger.LogDebug("Ignoring podcast episdoe [{episode}]", episode.DisplayString());
            }
            else if (e.Current.Item is null)
            {
                Logger.LogDebug("Skipping play count pulling for null item [{context}]", e.Current.DisplayString());
            }
            else
            {
                Logger.LogError("Unknown item pulled from API [{item}]", e.Current.Item);
            }
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange += Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }
    }
}

