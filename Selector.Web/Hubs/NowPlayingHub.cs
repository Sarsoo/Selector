using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using Selector.Cache;

namespace Selector.Web.Hubs
{
    public interface INowPlayingHubClient
    {
        public Task OnNewPlaying(CurrentlyPlayingDTO context);
    }

    public class NowPlayingHub: Hub<INowPlayingHubClient>
    {
        public Task SendNewPlaying(CurrentlyPlayingDTO context)
        {
            return Clients.All.OnNewPlaying(context);
        }
    }
}