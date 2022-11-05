using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Selector.Model;

namespace Selector.Cache
{
    public class DBPlayCountPuller
    {
        protected readonly IListenRepository ScrobbleRepository;
        private readonly IOptions<NowPlayingOptions> nowOptions;

        public DBPlayCountPuller(
            IOptions<NowPlayingOptions> options,
            IListenRepository scrobbleRepository
        )
        {
            ScrobbleRepository = scrobbleRepository;
            nowOptions = options;
        }

        public Task<PlayCount> Get(string username, string track, string artist, string album, string albumArtist)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException("No username provided");

            var userScrobbleCount = ScrobbleRepository.Count(username: username);

            var artistScrobbles = ScrobbleRepository.GetAll(username: username, artistName: artist, tracking: false, orderTime: true).ToArray();
            var albumScrobbles = artistScrobbles.Where(
                s => s.AlbumName.Equals(album, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            var trackScrobbles = artistScrobbles.Where(
                s => s.TrackName.Equals(track, StringComparison.CurrentCultureIgnoreCase)).ToArray();

            var postCalc = artistScrobbles.Resample(nowOptions.Value.ArtistResampleWindow).Select(s => s.Value).Sum();
            //var postCalc = playCount.ArtistCountData.Select(s => s.Value).Sum();
            Debug.Assert(postCalc == artistScrobbles.Count());

            PlayCount playCount = new()
            {
                Username = username,
                Artist = artistScrobbles.Count(),
                Album = albumScrobbles.Count(),
                Track = trackScrobbles.Count(),
                User = userScrobbleCount,

                ArtistCountData = artistScrobbles
                    .Resample(nowOptions.Value.ArtistResampleWindow)
                    //.ResampleByMonth()
                    .CumulativeSum()
                    .ToArray(),

                AlbumCountData = albumScrobbles
                    .Resample(nowOptions.Value.AlbumResampleWindow)
                    //.ResampleByMonth()
                    .CumulativeSum()
                    .ToArray(),

                TrackCountData = trackScrobbles
                    .Resample(nowOptions.Value.TrackResampleWindow)
                    //.ResampleByMonth()
                    .CumulativeSum()
                    .ToArray()
            };

            return Task.FromResult(playCount);
        }
    }
}
