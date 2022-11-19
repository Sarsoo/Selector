using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Objects;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Selector.Cache;
using Selector.Model;
using StackExchange.Redis;

namespace Selector.Web.Hubs
{
    public interface IPastHubClient
    {
        public Task OnRankResult(RankResult result);
    }

    public class PastHub: Hub<IPastHubClient>
    {
        private readonly IDatabaseAsync Cache;
        private readonly AudioFeaturePuller AudioFeaturePuller;
        private readonly PlayCountPuller PlayCountPuller;
        private readonly DBPlayCountPuller DBPlayCountPuller;
        private readonly ApplicationDbContext Db;
        private readonly IListenRepository ListenRepository;

        private readonly IOptions<PastOptions> pastOptions;

        public PastHub(
            IDatabaseAsync cache, 
            AudioFeaturePuller featurePuller, 
            ApplicationDbContext db,
            IListenRepository listenRepository,
            IOptions<PastOptions> options,
            DBPlayCountPuller dbPlayCountPuller,
            PlayCountPuller playCountPuller = null
        )
        {
            Cache = cache;
            AudioFeaturePuller = featurePuller;
            PlayCountPuller = playCountPuller;
            DBPlayCountPuller = dbPlayCountPuller;
            Db = db;
            ListenRepository = listenRepository;
            pastOptions = options;
        }

        public async Task OnConnected()
        {

        }

        private static IEnumerable<string> AlbumSuffixes = new[]
        {
            " (Deluxe)",
            " (Deluxe Edition)",
            " (Special)",
            " (Special Edition)",
            " (Expanded)",
            " (Expanded Edition)",
        };

        public async Task OnSubmitted(PastParams param)
        {
            param.Track = string.IsNullOrWhiteSpace(param.Track) ? null : param.Track;
            param.Album = string.IsNullOrWhiteSpace(param.Album) ? null : param.Album;
            param.Artist = string.IsNullOrWhiteSpace(param.Artist) ? null : param.Artist;

            DateTime? from = param.From is string f && DateTime.TryParse(f, out var fromDate) ? fromDate.ToUniversalTime() : null;
            DateTime? to = param.To is string t && DateTime.TryParse(t, out var toDate) ? toDate.ToUniversalTime() : null;

            var listenQuery = ListenRepository.GetAll(
                userId: Context.UserIdentifier,
                trackName: param.Track,
                albumName: param.Album,
                artistName: param.Artist,
                from: from,
                to: to
            ).ToArray();

            Parallel.ForEach(listenQuery, (listen) =>
            {
                foreach (var suffix in AlbumSuffixes)
                {
                    if (listen.AlbumName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        listen.AlbumName = listen.AlbumName.Substring(0, listen.AlbumName.Length - suffix.Length);
                    }
                }
            });

            var artistGrouped = listenQuery
                .GroupBy(x => x.ArtistName.ToLowerInvariant())
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(pastOptions.Value.RankingCount)
                .ToArray();

            var albumGrouped = listenQuery
                .GroupBy(x => (x.AlbumName.ToLowerInvariant(), x.ArtistName.ToLowerInvariant()))
                .Select(x => (x.Key, x.Count(), $"{x.FirstOrDefault()?.AlbumName} // {x.FirstOrDefault()?.ArtistName}"))
                .OrderByDescending(x => x.Item2)
                .Take(pastOptions.Value.RankingCount)
                .ToArray();

            var trackGrouped = listenQuery
                .GroupBy(x => (x.TrackName.ToLowerInvariant(), x.ArtistName.ToLowerInvariant()))
                .Select(x => (x.Key, x.Count(), $"{x.FirstOrDefault()?.TrackName} // {x.FirstOrDefault()?.ArtistName}"))
                .OrderByDescending(x => x.Item2)
                .Take(pastOptions.Value.RankingCount)
                .ToArray();

            await Clients.Caller.OnRankResult(new()
            {
                TrackEntries = trackGrouped.Select(x => new ChartEntry()
                {
                    Name = x.Item3,
                    Value = x.Item2
                }).ToArray(),

                AlbumEntries = albumGrouped.Select(x => new ChartEntry()
                {
                    Name = x.Item3,
                    Value = x.Item2
                }).ToArray(),

                ArtistEntries = artistGrouped.Select(x => new ChartEntry()
                {
                    Name = x.Key,
                    Value = x.Item2
                }).ToArray(),

                ResampledSeries = listenQuery
                    .Resample(pastOptions.Value.ResampleWindow)
                    //.ResampleByMonth()
                    .CumulativeSum()
                    .ToArray(),

                TotalCount = listenQuery.Length
            });
        }
    }
}