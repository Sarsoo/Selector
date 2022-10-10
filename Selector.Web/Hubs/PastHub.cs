using System;
using System.Linq;
using System.Threading.Tasks;
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

            var artistGrouped = listenQuery
                .GroupBy(x => x.ArtistName)
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(20)
                .ToArray();

            var albumGrouped = listenQuery
                .GroupBy(x => (x.AlbumName, x.ArtistName))
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(20)
                .ToArray();

            var trackGrouped = listenQuery
                .GroupBy(x => (x.TrackName, x.ArtistName))
                .Select(x => (x.Key, x.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(20)
                .ToArray();

            await Clients.Caller.OnRankResult(new()
            {
                TrackEntries = trackGrouped.Select(x => new ChartEntry()
                {
                    Name = $"{x.Key.TrackName} - {x.Key.ArtistName}",
                    Value = x.Item2
                }).ToArray(),

                AlbumEntries = albumGrouped.Select(x => new ChartEntry()
                {
                    Name = $"{x.Key.AlbumName} - {x.Key.ArtistName}",
                    Value = x.Item2
                }).ToArray(),

                ArtistEntries = artistGrouped.Select(x => new ChartEntry()
                {
                    Name = x.Key,
                    Value = x.Item2
                }).ToArray(),
            });
        }
    }
}