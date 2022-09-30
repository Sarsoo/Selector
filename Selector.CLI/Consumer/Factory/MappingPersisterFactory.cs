using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Selector.Model;

namespace Selector.CLI.Consumer
{
    public interface IMappingPersisterFactory
    {
        public Task<IPlayerConsumer> Get(IPlayerWatcher watcher = null);
    }
    
    public class MappingPersisterFactory : IMappingPersisterFactory
    {
        private readonly ILoggerFactory LoggerFactory;
        private readonly IServiceScopeFactory ScopeFactory;

        public MappingPersisterFactory(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory = null, LastFmCredentials creds = null)
        {
            LoggerFactory = loggerFactory;
            ScopeFactory = scopeFactory;
        }

        public Task<IPlayerConsumer> Get(IPlayerWatcher watcher = null)
        {
            return Task.FromResult<IPlayerConsumer>(new MappingPersister(
                watcher,
                ScopeFactory,
                LoggerFactory.CreateLogger<MappingPersister>()
            ));
        }
    }
}
