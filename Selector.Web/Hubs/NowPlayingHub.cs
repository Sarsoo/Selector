using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using SpotifyAPI.Web;
using StackExchange.Redis;

using Selector.Cache;
using System.Text.Json;

namespace Selector.Web.Hubs
{
    public interface INowPlayingHubClient
    {
        public Task OnNewPlaying(CurrentlyPlayingDTO context);
        // public Task OnNewAudioFeature(TrackAudioFeatures features);
    }

    public class NowPlayingHub: Hub<INowPlayingHubClient>
    {
        private readonly IDatabaseAsync Cache;
        // private readonly AudioFeaturePuller AudioFeaturePuller;

        public NowPlayingHub(IDatabaseAsync cache)
        {
            Cache = cache;
        }

        public async Task OnConnected()
        {
            await SendNewPlaying();
        }

        public async Task SendNewPlaying()
        {
            var nowPlaying = await Cache.StringGetAsync(Key.CurrentlyPlaying(Context.UserIdentifier));
            var deserialised = JsonSerializer.Deserialize<CurrentlyPlayingDTO>(nowPlaying);
            await Clients.Caller.OnNewPlaying(deserialised);
        }

        // public async Task SendAudioFeatures(string trackId)
        // {
        //     await Clients.Caller.OnNewAudioFeature(await AudioFeaturePuller.Get(trackId));
        // }
    }
}