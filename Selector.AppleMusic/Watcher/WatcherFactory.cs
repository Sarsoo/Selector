using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector.AppleMusic.Watcher
{
    public interface IAppleMusicWatcherFactory
    {
        Task<IWatcher> Get<T>(AppleMusicApiProvider appleMusicProvider, string developerToken, string teamId,
            string keyId, string userToken, string id = null, int pollPeriod = 10000)
            where T : class, IWatcher;
    }

    public class AppleMusicWatcherFactory : IAppleMusicWatcherFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IEqual Equal;

        public AppleMusicWatcherFactory(ILoggerFactory loggerFactory, IEqual equal)
        {
            LoggerFactory = loggerFactory;
            Equal = equal;
        }

        public async Task<IWatcher> Get<T>(AppleMusicApiProvider appleMusicProvider, string developerToken,
            string teamId, string keyId, string userToken, string id = null, int pollPeriod = 10000)
            where T : class, IWatcher
        {
            if (typeof(T).IsAssignableFrom(typeof(AppleMusicPlayerWatcher)))
            {
                if (!Magic.Dummy)
                {
                    var api = appleMusicProvider.GetApi(developerToken, teamId, keyId, userToken);

                    return new AppleMusicPlayerWatcher(
                        api,
                        LoggerFactory?.CreateLogger<AppleMusicPlayerWatcher>() ??
                        NullLogger<AppleMusicPlayerWatcher>.Instance,
                        pollPeriod: pollPeriod
                    )
                    {
                        Id = id
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new ArgumentException("Type unsupported");
            }
        }
    }
}