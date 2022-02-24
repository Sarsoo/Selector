using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Selector.Model
{
    public class ScrobbleMappingRepository : IScrobbleMappingRepository
    {
        private readonly ApplicationDbContext db;

        public ScrobbleMappingRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public void Add(TrackLastfmSpotifyMapping item)
        {
            db.TrackMapping.Add(item);
        }

        public void Add(AlbumLastfmSpotifyMapping item)
        {
            db.AlbumMapping.Add(item);
        }

        public void Add(ArtistLastfmSpotifyMapping item)
        {
            db.ArtistMapping.Add(item);
        }

        public void AddRange(IEnumerable<TrackLastfmSpotifyMapping> item)
        {
            db.TrackMapping.AddRange(item);
        }

        public void AddRange(IEnumerable<AlbumLastfmSpotifyMapping> item)
        {
            db.AlbumMapping.AddRange(item);
        }

        public void AddRange(IEnumerable<ArtistLastfmSpotifyMapping> item)
        {
            db.ArtistMapping.AddRange(item);
        }

        public IEnumerable<AlbumLastfmSpotifyMapping> GetAlbums(string include = null, string albumName = null, string artistName = null)
        {
            var mappings = db.AlbumMapping.AsQueryable();

            if (!string.IsNullOrWhiteSpace(include))
            {
                mappings = mappings.Include(include);
            }

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                mappings = mappings.Where(s => s.LastfmAlbumName == albumName);
            }

            if (!string.IsNullOrWhiteSpace(artistName))
            {
                mappings = mappings.Where(s => s.LastfmArtistName == artistName);
            }

            return mappings.AsEnumerable();
        }

        public IEnumerable<ArtistLastfmSpotifyMapping> GetArtists(string include = null, string artistName = null)
        {
            var mappings = db.ArtistMapping.AsQueryable();

            if (!string.IsNullOrWhiteSpace(include))
            {
                mappings = mappings.Include(include);
            }

            if (!string.IsNullOrWhiteSpace(artistName))
            {
                mappings = mappings.Where(s => s.LastfmArtistName == artistName);
            }

            return mappings.AsEnumerable();
        }

        public IEnumerable<TrackLastfmSpotifyMapping> GetTracks(string include = null, string trackName = null, string albumName = null, string artistName = null)
        {
            var mappings = db.TrackMapping.AsQueryable();

            if (!string.IsNullOrWhiteSpace(include))
            {
                mappings = mappings.Include(include);
            }

            if (!string.IsNullOrWhiteSpace(trackName))
            {
                mappings = mappings.Where(s => s.LastfmTrackName == trackName);
            }

            if (!string.IsNullOrWhiteSpace(artistName))
            {
                mappings = mappings.Where(s => s.LastfmArtistName == artistName);
            }

            return mappings.AsEnumerable();
        }

        public void Remove(TrackLastfmSpotifyMapping mapping)
        {
            db.TrackMapping.Remove(mapping);
        }

        public void Remove(AlbumLastfmSpotifyMapping mapping)
        {
            db.AlbumMapping.Remove(mapping);
        }

        public void Remove(ArtistLastfmSpotifyMapping mapping)
        {
            db.ArtistMapping.Remove(mapping);
        }

        public void RemoveRange(IEnumerable<TrackLastfmSpotifyMapping> mappings)
        {
            db.TrackMapping.RemoveRange(mappings);
        }

        public void RemoveRange(IEnumerable<AlbumLastfmSpotifyMapping> mappings)
        {
            db.AlbumMapping.RemoveRange(mappings);
        }

        public void RemoveRange(IEnumerable<ArtistLastfmSpotifyMapping> mappings)
        {
            db.ArtistMapping.RemoveRange(mappings);
        }

        public Task<int> Save()
        {
            return db.SaveChangesAsync();
        }
    }
}
