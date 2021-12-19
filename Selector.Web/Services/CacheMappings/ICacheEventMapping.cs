using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Selector.Web.Service
{
    public interface ICacheEventMapping
    {
        public Task ConstructMapping();
    }
}