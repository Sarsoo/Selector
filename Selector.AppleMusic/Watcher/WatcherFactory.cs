using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Selector.AppleMusic.Watcher
{
    public interface IAppleMusicWatcherFactory
    {
        Task<IWatcher> Get<T>(AppleMusicApiProvider appleMusicProvider, string developerToken, string teamId,
            string keyId, string userToken, string id, int pollPeriod = 3000)
            where T : class, IWatcher;
    }

    public class AppleMusicWatcherFactory(ILoggerFactory loggerFactory) : IAppleMusicWatcherFactory
    {
        public Task<IWatcher> Get<T>(AppleMusicApiProvider appleMusicProvider, string developerToken,
            string teamId, string keyId, string userToken, string id, int pollPeriod = 3000)
            where T : class, IWatcher
        {
            if (typeof(T).IsAssignableFrom(typeof(AppleMusicPlayerWatcher)))
            {
                if (!Magic.Dummy)
                {
                    var api = appleMusicProvider.GetApi(developerToken, teamId, keyId, userToken);

                    return Task.FromResult<IWatcher>(new AppleMusicPlayerWatcher(
                        api,
                        loggerFactory?.CreateLogger<AppleMusicPlayerWatcher>() ??
                        NullLogger<AppleMusicPlayerWatcher>.Instance,
                        pollPeriod: pollPeriod
                    )
                    {
                        Id = id
                    });
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