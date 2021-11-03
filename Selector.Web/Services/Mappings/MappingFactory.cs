using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SpotifyAPI.Web;

namespace Selector.Web.Service
{
    public interface INowPlayingMappingFactory {
        public NowPlayingMapping Get(string userId);
    }

    public class NowPlayingMappingFactory : INowPlayingMappingFactory {

        private readonly ILoggerFactory LoggerFactory;

        public NowPlayingMappingFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public NowPlayingMapping Get(string userId)
        {
            return new NowPlayingMapping(
                LoggerFactory?.CreateLogger<NowPlayingMapping>(),
                userId
            );
        }
    }
}
