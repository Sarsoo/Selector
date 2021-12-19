using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;

namespace Selector.Web.Service
{
    public interface IEventHubMapping<THub, T>
        where THub : Hub<T>
        where T : class
    {
        public Task ConstructMapping();
        // public Task RemoveMapping();
    }
}
