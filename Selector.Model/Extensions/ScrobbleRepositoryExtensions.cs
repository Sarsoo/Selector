using System;

namespace Selector.Model.Extensions
{
    public static class ScrobbleRepositoryExtensions
    {
        public static int CountToday(this IScrobbleRepository repo, string userId = null, string username = null, string track = null, string album = null, string artist = null)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return repo.Count(userId: userId, from: DateTime.Now.ToUniversalTime().Date,
                    artistName: artist,
                    albumName: album,
                    trackName: track);
            }
            else if (!string.IsNullOrWhiteSpace(username))
            {
                return repo.Count(username: username, from: DateTime.Now.ToUniversalTime().Date,
                    artistName: artist,
                    albumName: album,
                    trackName: track);
            }
            else
            {
                throw new ArgumentNullException("user");
            }
        }
    }
}
