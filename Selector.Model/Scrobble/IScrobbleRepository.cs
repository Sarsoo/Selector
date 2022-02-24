using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selector.Model
{
    public interface IScrobbleRepository
    {
        void Add(UserScrobble item);
        void AddRange(IEnumerable<UserScrobble> item);
        IEnumerable<UserScrobble> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
        UserScrobble Find(int key, string include = null);
        void Remove(int key);
        public void Remove(UserScrobble scrobble);
        public void RemoveRange(IEnumerable<UserScrobble> scrobbles);
        void Update(UserScrobble item);
        Task<int> Save();
    }
}
