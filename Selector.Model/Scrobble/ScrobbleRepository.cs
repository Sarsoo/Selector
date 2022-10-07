using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Selector.Model
{
    public class ScrobbleRepository : IScrobbleRepository
    {
        private readonly ApplicationDbContext db;

        public ScrobbleRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public void Add(UserScrobble item)
        {
            db.Scrobble.Add(item);
        }

        public void AddRange(IEnumerable<UserScrobble> item)
        {
            db.Scrobble.AddRange(item);
        }

        public UserScrobble Find(int key, string include = null)
        {
            var scrobbles = db.Scrobble.Where(s => s.Id == key);
            
            if (!string.IsNullOrWhiteSpace(include))
            {
                scrobbles = scrobbles.Include(include);
            }
                
            return scrobbles.FirstOrDefault();
        }

        private IQueryable<UserScrobble> GetAllQueryable(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null)
        {
            var scrobbles = db.Scrobble.AsQueryable();

            if (!string.IsNullOrWhiteSpace(include))
            {
                scrobbles = scrobbles.Include(include);
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                scrobbles = scrobbles.Where(s => s.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                var normalUsername = username.ToUpperInvariant();
                var user = db.Users.AsNoTracking().Where(u => u.NormalizedUserName == normalUsername).FirstOrDefault();
                if (user is not null)
                {
                    scrobbles = scrobbles.Where(s => s.UserId == user.Id);
                }
                else
                {
                    scrobbles = Enumerable.Empty<UserScrobble>().AsQueryable();
                }
            }

            if (!string.IsNullOrWhiteSpace(trackName))
            {
                scrobbles = scrobbles.Where(s => s.TrackName == trackName);
            }

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                scrobbles = scrobbles.Where(s => s.AlbumName == albumName);
            }

            if (!string.IsNullOrWhiteSpace(artistName))
            {
                scrobbles = scrobbles.Where(s => s.ArtistName == artistName);
            }

            if (from is not null)
            {
                scrobbles = scrobbles.Where(u => u.Timestamp >= from.Value);
            }

            if (to is not null)
            {
                scrobbles = scrobbles.Where(u => u.Timestamp < to.Value);
            }

            return scrobbles;
        }

        public IEnumerable<IListen> GetAll(string include = null, string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null)
            => GetAllQueryable(include: include, userId: userId, username: username, trackName: trackName, albumName: albumName, artistName: artistName, from: from, to: to).AsEnumerable();

        public void Remove(int key)
        {
            Remove(Find(key));
        }

        public void Remove(UserScrobble scrobble)
        {
            db.Scrobble.Remove(scrobble);
        }

        public void RemoveRange(IEnumerable<UserScrobble> scrobbles)
        {
            db.Scrobble.RemoveRange(scrobbles);
        }

        public void Update(UserScrobble item)
        {
            db.Scrobble.Update(item);
        }

        public Task<int> Save()
        {
            return db.SaveChangesAsync();
        }

        public int Count(string userId = null, string username = null, string trackName = null, string albumName = null, string artistName = null, DateTime? from = null, DateTime? to = null)
            => GetAllQueryable(userId: userId, username: username, trackName: trackName, albumName: albumName, artistName: artistName, from: from, to: to).Count();
    }
}
