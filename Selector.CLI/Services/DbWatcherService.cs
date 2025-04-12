using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer.Factory;
using Selector.AppleMusic.Watcher;
using Selector.Cache;
using Selector.CLI.Consumer;
using Selector.Events;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using Selector.Spotify.Consumer.Factory;
using Selector.Spotify.FactoryProvider;
using Selector.Spotify.Watcher;

namespace Selector.CLI
{
    class DbWatcherService(
        ISpotifyWatcherFactory spotifyWatcherFactory,
        IAppleMusicWatcherFactory appleWatcherFactory,
        IWatcherCollectionFactory watcherCollectionFactory,
        IRefreshTokenFactoryProvider factory,
        AppleMusicApiProvider appleMusicProvider,
        // IAudioFeatureInjectorFactory audioFeatureInjectorFactory,
        IAppleMusicScrobblerFactory scrobblerFactory,
        IPlayCounterFactory playCounterFactory,
        UserEventBus userEventBus,
        ILogger<DbWatcherService> logger,
        IOptions<AppleMusicOptions> appleMusicOptions,
        IServiceProvider serviceProvider,
        IPublisherFactory publisherFactory = null,
        ICacheWriterFactory cacheWriterFactory = null,
        IMappingPersisterFactory mappingPersisterFactory = null,
        IUserEventFirerFactory userEventFirerFactory = null)
        : IHostedService
    {
        private const int PollPeriod = 1000;

        // private readonly IAudioFeatureInjectorFactory AudioFeatureInjectorFactory = audioFeatureInjectorFactory;

        private ConcurrentDictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting database watcher service...");

            var watcherIndices = await InitInstances();
            AttachEventBus();

            logger.LogInformation("Starting {count} affected watcher collection(s)...", watcherIndices.Count());
            StartWatcherCollections(watcherIndices);
        }

        private async Task<IEnumerable<string>> InitInstances()
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var indices = new HashSet<string>();

            foreach (var dbWatcher in db.Watcher
                         .Include(w => w.User)
                         .Where(w =>
                             ((w.Type == WatcherType.SpotifyPlayer || w.Type == WatcherType.SpotifyPlaylist) &&
                              !string.IsNullOrWhiteSpace(w.User.SpotifyRefreshToken)) ||
                             (w.Type == WatcherType.AppleMusicPlayer && w.User.AppleMusicLinked)
                         ))
            {
                using var logScope = logger.BeginScope(new Dictionary<string, string>
                {
                    { "username", dbWatcher.User.UserName }
                });

                var watcherCollectionIdx = dbWatcher.UserId;
                indices.Add(watcherCollectionIdx);

                await InitInstance(dbWatcher);
            }

            return indices;
        }

        private async Task<IWatcherContext> InitInstance(Watcher dbWatcher)
        {
            logger.LogInformation("Creating new [{type}] watcher", dbWatcher.Type);

            var watcherCollectionIdx = dbWatcher.UserId;

            if (!Watchers.ContainsKey(watcherCollectionIdx))
                Watchers[watcherCollectionIdx] = watcherCollectionFactory.Get();

            var watcherCollection = Watchers[watcherCollectionIdx];

            IWatcher watcher = null;
            List<IConsumer> consumers = new();

            switch (dbWatcher.Type)
            {
                case WatcherType.SpotifyPlayer:
                    logger.LogDebug("Getting Spotify factory");
                    var spotifyFactory = await factory.GetFactory(dbWatcher.User.SpotifyRefreshToken);

                    watcher = await spotifyWatcherFactory.Get<SpotifyPlayerWatcher>(spotifyFactory,
                        id: dbWatcher.UserId, pollPeriod: PollPeriod);

                    // deprecated, thanks Spotify!
                    // consumers.Add(await AudioFeatureInjectorFactory.Get(spotifyFactory));
                    if (cacheWriterFactory is not null) consumers.Add(await cacheWriterFactory.GetSpotify());
                    if (publisherFactory is not null) consumers.Add(await publisherFactory.GetSpotify());

                    if (mappingPersisterFactory is not null && !Magic.Dummy)
                        consumers.Add(await mappingPersisterFactory.Get());

                    if (userEventFirerFactory is not null) consumers.Add(await userEventFirerFactory.GetSpotify());

                    if (dbWatcher.User.LastFmConnected())
                    {
                        consumers.Add(await playCounterFactory.Get(creds: new()
                            { Username = dbWatcher.User.LastFmUsername }));
                    }
                    else
                    {
                        logger.LogDebug("[{username}] No Last.fm username, skipping play counter",
                            dbWatcher.User.UserName);
                    }

                    break;

                case WatcherType.SpotifyPlaylist:
                    throw new NotImplementedException("Playlist watchers not implemented");
                // break;
                case WatcherType.AppleMusicPlayer:
                    watcher = await appleWatcherFactory.Get<AppleMusicPlayerWatcher>(appleMusicProvider,
                        appleMusicOptions.Value.Key, appleMusicOptions.Value.TeamId, appleMusicOptions.Value.KeyId,
                        dbWatcher.User.AppleMusicKey, id: dbWatcher.UserId);

                    if (cacheWriterFactory is not null) consumers.Add(await cacheWriterFactory.GetApple());
                    if (publisherFactory is not null) consumers.Add(await publisherFactory.GetApple());

                    if (userEventFirerFactory is not null) consumers.Add(await userEventFirerFactory.GetApple());

                    if (dbWatcher.User.LastFmConnected() && !string.IsNullOrWhiteSpace(dbWatcher.User.LastFmPassword))
                    {
                        var scrobbler = await scrobblerFactory.Get();
                        await scrobbler.Auth(dbWatcher.User.LastFmUsername, dbWatcher.User.LastFmPassword);
                        consumers.Add(scrobbler);
                    }
                    else
                    {
                        logger.LogDebug("[{username}] No Last.fm username/password, skipping scrobbler",
                            dbWatcher.User.UserName);
                    }

                    break;
            }

            return watcherCollection.Add(watcher, consumers);
        }

        private void StartWatcherCollections(IEnumerable<string> indices)
        {
            foreach (var index in indices)
            {
                try
                {
                    logger.LogInformation("Starting watcher collection [{index}]", index);
                    Watchers[index].Start();
                }
                catch (KeyNotFoundException)
                {
                    logger.LogError("Unable to retrieve watcher collection [{index}] when starting", index);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Shutting down");

            foreach ((var key, var watcher) in Watchers)
            {
                logger.LogInformation("Stopping watcher collection [{key}]", key);
                watcher.Stop();
            }

            DetachEventBus();

            return Task.CompletedTask;
        }

        private void AttachEventBus()
        {
            userEventBus.SpotifyLinkChange += SpotifyChangeCallback;
            userEventBus.AppleLinkChange += AppleMusicChangeCallback;
            userEventBus.LastfmCredChange += LastfmChangeCallback;
        }

        private void DetachEventBus()
        {
            userEventBus.SpotifyLinkChange -= SpotifyChangeCallback;
            userEventBus.AppleLinkChange -= AppleMusicChangeCallback;
            userEventBus.LastfmCredChange -= LastfmChangeCallback;
        }

        public async void SpotifyChangeCallback(object sender, SpotifyLinkChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                logger.LogDebug("Setting new Spotify link state for [{username}], [{}]", change.UserId,
                    change.NewLinkState);

                var watcherCollection = Watchers[change.UserId];

                if (change.NewLinkState)
                {
                    watcherCollection.Start();
                }
                else
                {
                    watcherCollection.Stop();
                }
            }
            else
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var watcherEnum = db.Watcher
                    .Include(w => w.User)
                    .Where(w => w.UserId == change.UserId);

                foreach (var dbWatcher in watcherEnum)
                {
                    var context = await InitInstance(dbWatcher);
                }

                Watchers[change.UserId].Start();

                logger.LogDebug("Started {} watchers for [{username}]", watcherEnum.Count(), change.UserId);
            }
        }

        public async void AppleMusicChangeCallback(object sender, AppleMusicLinkChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                logger.LogDebug("Setting new Apple Music link state for [{username}], [{}]", change.UserId,
                    change.NewLinkState);

                var watcherCollection = Watchers[change.UserId];

                if (change.NewLinkState)
                {
                    watcherCollection.Start();
                }
                else
                {
                    watcherCollection.Stop();
                }
            }
            else
            {
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var watcherEnum = db.Watcher
                    .Include(w => w.User)
                    .Where(w => w.UserId == change.UserId);

                foreach (var dbWatcher in watcherEnum)
                {
                    var context = await InitInstance(dbWatcher);
                }

                Watchers[change.UserId].Start();

                logger.LogDebug("Started {} watchers for [{username}]", watcherEnum.Count(), change.UserId);
            }
        }

        public void LastfmChangeCallback(object sender, LastfmChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                logger.LogDebug("Setting new username for [{}], [{}]", change.UserId, change.NewUsername);

                var watcherCollection = Watchers[change.UserId];

                foreach (var watcher in watcherCollection.Consumers)
                {
                    if (watcher is PlayCounter counter)
                    {
                        counter.Credentials.Username = change.NewUsername;
                    }
                }
            }
            else
            {
                logger.LogDebug("No watchers running for [{username}], skipping Spotify event", change.UserId);
            }
        }
    }
}