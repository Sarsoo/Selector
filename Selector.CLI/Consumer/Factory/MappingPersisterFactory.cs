using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Selector.CLI.Consumer
{
    public interface IMappingPersisterFactory
    {
        public Task<ISpotifyPlayerConsumer> Get(ISpotifyPlayerWatcher watcher = null);
    }

    public class MappingPersisterFactory : IMappingPersisterFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IServiceScopeFactory ScopeFactory;

        public MappingPersisterFactory(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory = null,
            LastFmCredentials creds = null)
        {
            LoggerFactory = loggerFactory;
            ScopeFactory = scopeFactory;
        }

        public Task<ISpotifyPlayerConsumer> Get(ISpotifyPlayerWatcher watcher = null)
        {
            return Task.FromResult<ISpotifyPlayerConsumer>(new MappingPersister(
                watcher,
                ScopeFactory,
                LoggerFactory.CreateLogger<MappingPersister>()
            ));
        }
    }
}