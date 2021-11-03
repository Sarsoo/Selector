using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

using StackExchange.Redis;
using Selector.Web.Hubs;

namespace Selector.Web.Service
{
    public class CacheHubProxy
    {
        private readonly ILogger<CacheHubProxy> Logger;
        private readonly ISubscriber Subscriber;
        private readonly ServiceProvider Services;

        public CacheHubProxy(ILogger<CacheHubProxy> logger,
            ISubscriber subscriber,
            ServiceProvider services
        )
        {
            Logger = logger;
            Subscriber = subscriber;
            Services = services;
        }

        public void FormMapping<THub, T>(ICacheHubMapping<THub, T> mapping) where THub: Hub<T> where T: class
        {
            var context = Services.GetService<IHubContext<THub, T>>();
            mapping.ConstructMapping(Subscriber, context);
        }
    }

    // public class CacheHubProxy<THub, T> 
    //     where THub: Hub<T> 
    //     where T: class
    // {
    //     private readonly ILogger<CacheHubProxy<THub, T>> Logger;
    //     private readonly ISubscriber Subscriber;
    //     private readonly IHubContext<THub, T> HubContext;
    //     private readonly List<ICacheHubMapping<THub, T>> Mappings;

    //     public CacheHubProxy(ILogger<CacheHubProxy<THub, T>> logger,
    //         ISubscriber subscriber,
    //         IHubContext<THub, T> hubContext,
    //         IEnumerable<ICacheHubMapping<THub, T>> mappings 
    //     )
    //     {
    //         Logger = logger;
    //         Subscriber = subscriber;
    //         HubContext = hubContext;
    //         Mappings = mappings.ToList();
    //     }

    //     public void FormMapping(ICacheHubMapping<THub, T> mapping)
    //     {
    //         mapping.ConstructMapping(Subscriber, HubContext);
    //     }

    //     public void AddMapping(ICacheHubMapping<THub, T> mapping)
    //     {
    //         Mappings.Add(mapping);
    //         FormMapping(mapping);
    //     }
    // }
}