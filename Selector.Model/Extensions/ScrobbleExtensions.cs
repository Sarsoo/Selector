
namespace Selector.Model.Extensions
{
    public static class ScrobbleExtensions
    {
        /// <summary>
        /// Helper to get the <see cref="Scrobble.AlbumArtistName"/> if possible, if not fall back to artist name 
        /// </summary>
        /// <param name="scrobble"></param>
        /// <returns></returns>
        public static string AlbumArtist(this Scrobble scrobble) => scrobble.AlbumArtistName ?? scrobble.ArtistName;
    }
}
