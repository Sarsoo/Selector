using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Selector.Events
{
    public interface IEventMapping
    {
        public Task ConstructMapping();
    }
}