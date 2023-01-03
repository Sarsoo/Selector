using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.Model
{
    public interface IScrobbleMappingRepository
    {
        void Add(TrackLastfmSpotifyMapping item);
        void Add(AlbumLastfmSpotifyMapping item);
        void Add(ArtistLastfmSpotifyMapping item);
        void AddRange(IEnumerable<TrackLastfmSpotifyMapping> item);
        void AddRange(IEnumerable<AlbumLastfmSpotifyMapping> item);
        void AddRange(IEnumerable<ArtistLastfmSpotifyMapping> item);
        IQueryable<TrackLastfmSpotifyMapping> GetTracks(string include = null, string trackName = null, string albumName = null, string artistName = null);
        IQueryable<AlbumLastfmSpotifyMapping> GetAlbums(string include = null, string albumName = null, string artistName = null);
        IQueryable<ArtistLastfmSpotifyMapping> GetArtists(string include = null, string artistName = null);
        
        public void Remove(TrackLastfmSpotifyMapping mapping);
        public void Remove(AlbumLastfmSpotifyMapping mapping);
        public void Remove(ArtistLastfmSpotifyMapping mapping);
        public void RemoveRange(IEnumerable<TrackLastfmSpotifyMapping> mappings);
        public void RemoveRange(IEnumerable<AlbumLastfmSpotifyMapping> mappings);
        public void RemoveRange(IEnumerable<ArtistLastfmSpotifyMapping> mappings);
        Task<int> Save();
    }
}
