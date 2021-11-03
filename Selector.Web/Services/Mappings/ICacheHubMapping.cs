using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Selector.Web.Service
{
    public interface ICacheHubMapping<THub, T> 
        where THub : Hub<T>
        where T : class
    {
        public Task ConstructMapping(ISubscriber subscriber, IHubContext<THub, T> hub);
        // public Task RemoveMapping(ISubscriber subscriber, THub hub);
    }
}