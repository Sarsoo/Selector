using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Selector.Model
{
    public interface IAppleListenRepository : IListenRepository
    {
        void Add(AppleMusicListen item);

        void AddRange(IEnumerable<AppleMusicListen> item);

        //IEnumerable<AppleMusicListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
        AppleMusicListen Find(DateTime key, string include = null);
        void Remove(DateTime key);
        public void Remove(AppleMusicListen scrobble);
        public void RemoveRange(IEnumerable<AppleMusicListen> scrobbles);
        void Update(AppleMusicListen item);

        Task<int> Save();
        //int Count(string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null);
    }
}