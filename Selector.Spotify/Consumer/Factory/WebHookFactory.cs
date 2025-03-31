using Microsoft.Extensions.Logging;

namespace Selector.Spotify.Consumer.Factory
{
    public interface IWebHookFactory
    {
        public Task<WebHook> Get(WebHookConfig config, ISpotifyPlayerWatcher watcher = null);
    }

    public class WebHookFactory : IWebHookFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly HttpClient Http;

        public WebHookFactory(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            LoggerFactory = loggerFactory;
            Http = httpClient;
        }

        public Task<WebHook> Get(WebHookConfig config, ISpotifyPlayerWatcher watcher = null)
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