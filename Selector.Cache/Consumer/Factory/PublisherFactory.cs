﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Selector.Cache
{
    public interface IPublisherFactory {
        public Task<IPlayerConsumer> Get(IPlayerWatcher watcher = null);
    }

    public class PublisherFactory: IPublisherFactory {

        private readonly ILoggerFactory LoggerFactory;
        private readonly ISubscriber Subscriber;

        public PublisherFactory(
            ISubscriber subscriber, 
            ILoggerFactory loggerFactory
        ) {
            Subscriber = subscriber;
            LoggerFactory = loggerFactory;
        }

        public Task<IPlayerConsumer> Get(IPlayerWatcher watcher = null)
        {
            return Task.FromResult<IPlayerConsumer>(new Publisher(
                watcher,
                Subscriber,
                LoggerFactory.CreateLogger<Publisher>()
            ));
        }
    }
}
