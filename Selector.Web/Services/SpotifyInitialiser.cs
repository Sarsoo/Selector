using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Selector.Web.Service {
    public class SpotifyInitialiser : IHostedService
    {
        private readonly ILogger<SpotifyInitialiser> Logger;
        private readonly IRefreshTokenFactoryProvider FactoryProvider;
        private readonly RootOptions Config;

        public SpotifyInitialiser(
            ILogger<SpotifyInitialiser> logger,
            IRefreshTokenFactoryProvider factoryProvider,
            IOptions<RootOptions> config
        )
        {
            Logger = logger;
            FactoryProvider = factoryProvider;
            Config = config.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Initialising Spotify Factory");

            if(string.IsNullOrEmpty(Config.ClientId) || string.IsNullOrEmpty(Config.ClientSecret))
            {
                Logger.LogError("Unable to initialise Spotify factory, null client id or secret");
            }
            else 
            {
                FactoryProvider.Initialise(Config.ClientId, Config.ClientSecret);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}