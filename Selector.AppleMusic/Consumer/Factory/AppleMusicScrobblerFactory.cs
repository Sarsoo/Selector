using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;

namespace Selector.AppleMusic.Consumer.Factory
{
    public interface IAppleMusicScrobblerFactory
    {
        public Task<AppleMusicScrobbler> Get(IAppleMusicPlayerWatcher? watcher = null);
    }

    public class AppleMusicScrobblerFactory : IAppleMusicScrobblerFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly LastfmClient _lastClient;

        public AppleMusicScrobblerFactory(ILoggerFactory loggerFactory, LastfmClient lastClient)
        {
            _loggerFactory = loggerFactory;
            _lastClient = lastClient;
        }

        public Task<AppleMusicScrobbler> Get(IAppleMusicPlayerWatcher? watcher = null)
        {
            return Task.FromResult(new AppleMusicScrobbler(
                watcher,
                _lastClient,
                _loggerFactory.CreateLogger<AppleMusicScrobbler>()
            ));
        }
    }
}