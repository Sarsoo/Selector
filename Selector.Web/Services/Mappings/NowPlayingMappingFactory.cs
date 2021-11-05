using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using Selector.Web.Hubs;

namespace Selector.Web.Service
{
    public interface IUserMappingFactory<out TMap, THub, T>
        where TMap : ICacheHubMapping<THub, T> 
        where THub : Hub<T>
        where T : class
    {
        public TMap Get(string userId, string username);
    }

    public interface INowPlayingMappingFactory: IUserMappingFactory<NowPlayingMapping, NowPlayingHub, INowPlayingHubClient>
    { }
    
    public class NowPlayingMappingFactory : INowPlayingMappingFactory {

        private readonly ILoggerFactory LoggerFactory;

        public NowPlayingMappingFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public NowPlayingMapping Get(string userId, string username)
        {
            return new NowPlayingMapping(
                LoggerFactory?.CreateLogger<NowPlayingMapping>(),
                userId,
                username
            );
        }
    }
}
