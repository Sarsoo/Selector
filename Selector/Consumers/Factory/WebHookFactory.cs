using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using System.Net.Http;

namespace Selector
{
    public interface IWebHookFactory
    {
        public Task<WebHook> Get(WebHookConfig config, IPlayerWatcher watcher = null);
    }
    
    public class WebHookFactory: IWebHookFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly HttpClient Http;

        public WebHookFactory(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            LoggerFactory = loggerFactory;
            Http = httpClient;
        }

        public Task<WebHook> Get(WebHookConfig config, IPlayerWatcher watcher = null)
        {
            return Task.FromResult(new WebHook(
                watcher,
                Http,
                config,
                LoggerFactory.CreateLogger<WebHook>()
            ));
        }
    }
}
