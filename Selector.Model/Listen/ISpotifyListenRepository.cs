using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Selector.Model
{
    public interface ISpotifyListenRepository
    {
        void Add(SpotifyListen item);
        void AddRange(IEnumerable<SpotifyListen> item);
        IEnumerable<SpotifyListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
        SpotifyListen Find(DateTime key, string include = null);
        void Remove(DateTime key);
        public void Remove(SpotifyListen scrobble);
        public void RemoveRange(IEnumerable<SpotifyListen> scrobbles);
        void Update(SpotifyListen item);
        Task<int> Save();
        int Count(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
    }
}

