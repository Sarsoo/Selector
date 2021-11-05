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
        private readonly IServiceProvider Services;

        public CacheHubProxy(ILogger<CacheHubProxy> logger,
            ISubscriber subscriber,
            IServiceProvider services
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
}