using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;

using Selector.Cache;
using System.Text.Json;

namespace Selector.Web.Hubs
{
    public interface INowPlayingHubClient
    {
        public Task OnNewPlaying(CurrentlyPlayingDTO context);
    }

    public class NowPlayingHub: Hub<INowPlayingHubClient>
    {
        private readonly IDatabaseAsync Cache;

        public NowPlayingHub(IDatabaseAsync cache)
        {
            Cache = cache;
        }

        public async Task SendNewPlaying()
        {
            var nowPlaying = await Cache.StringGetAsync(Key.CurrentlyPlaying(Context.UserIdentifier));
            var deserialised = JsonSerializer.Deserialize<CurrentlyPlayingDTO>(nowPlaying);
            await Clients.Caller.OnNewPlaying(deserialised);
        }
    }
}