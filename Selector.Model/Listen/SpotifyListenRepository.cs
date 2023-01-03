using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Selector.Model
{
    public class SpotifyListenRepository : ISpotifyListenRepository
    {
        private readonly ApplicationDbContext db;

        public SpotifyListenRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public void Add(SpotifyListen item)
        {
            db.SpotifyListen.Add(item);
        }

        public void AddRange(IEnumerable<SpotifyListen> item)
        {
            db.SpotifyListen.AddRange(item);
        }

        public SpotifyListen Find(DateTime key, string include = null)
        {
            var listens = db.SpotifyListen.Where(s => s.Timestamp == key);
            
            if (!string.IsNullOrWhiteSpace(include))
            {
                listens = listens.Include(include);
            }
                
            return listens.FirstOrDefault();
        }

        public IQueryable<IListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null, bool tracking = true, bool orderTime = false)
        {
            var listens = db.SpotifyListen.AsQueryable();

            if (!tracking)
            {
                listens = listens.AsNoTracking();
            }

            if (!string.IsNullOrWhiteSpace(include))
            {
                listens = listens.Include(include);
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                listens = listens.Where(s => s.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                var normalUsername = username.ToUpperInvariant();
                var user = db.Users.AsNoTracking().Where(u => u.NormalizedUserName == normalUsername).FirstOrDefault();
                if (user is not null)
                {
                    listens = listens.Where(s => s.UserId == user.Id);
                }
                else
                {
                    listens = Enumerable.Empty<SpotifyListen>().AsQueryable();
                }
            }

            if (!string.IsNullOrWhiteSpace(trackName))
            {
                listens = listens.Where(s => s.TrackName == trackName);
            }

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                listens = listens.Where(s => s.AlbumName == albumName);
            }

            if (!string.IsNullOrWhiteSpace(artistName))
            {
                listens = listens.Where(s => s.ArtistName == artistName);
            }

            if (from is not null)
            {
                listens = listens.Where(u => u.Timestamp >= from.Value);
            }

            if (to is not null)
            {
                listens = listens.Where(u => u.Timestamp < to.Value);
            }

            if (orderTime)
            {
                listens = listens.OrderBy(x => x.Timestamp);
            }

            return listens;
        }

        // public IEnumerable<IListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null, bool tracking = true, bool orderTime = false)
        //     => GetAllQueryable(include: include, userId: userId, username: username, trackName: trackName, albumName: albumName, artistName: artistName, from: from, to: to, tracking: tracking, orderTime: orderTime).AsEnumerable();

        public void Remove(DateTime key)
        {
            Remove(Find(key));
        }

        public void Remove(SpotifyListen scrobble)
        {
            db.SpotifyListen.Remove(scrobble);
        }

        public void RemoveRange(IEnumerable<SpotifyListen> scrobbles)
        {
            db.SpotifyListen.RemoveRange(scrobbles);
        }

        public void Update(SpotifyListen item)
        {
            db.SpotifyListen.Update(item);
        }

        public Task<int> Save()
        {
            return db.SaveChangesAsync();
        }

        public int Count(string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null)
            => GetAll(userId: userId, username: username, trackName: trackName, albumName: albumName, artistName: artistName, from: from, to: to, tracking: false).Count();
    }
}
