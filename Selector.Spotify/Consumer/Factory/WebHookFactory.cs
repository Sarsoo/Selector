using Microsoft.Extensions.Logging;

namespace Selector.Spotify.Consumer.Factory
{
    public interface IWebHookFactory
    {
        public Task<WebHook> Get(WebHookConfig config, ISpotifyPlayerWatcher? watcher = null);
    }

    public class WebHookFactory : IWebHookFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly HttpClient _http;

        public WebHookFactory(ILoggerFactory loggerFactory, HttpClient httpClient)
        {
            _loggerFactory = loggerFactory;
            _http = httpClient;
        }

        public Task<WebHook> Get(WebHookConfig config, ISpotifyPlayerWatcher? watcher = null)
        {
            return Task.FromResult(new WebHook(
                watcher,
                _http,
                config,
                _loggerFactory.CreateLogger<WebHook>()
            ));
        }
    }
}