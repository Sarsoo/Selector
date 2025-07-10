using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;
using Selector.Cache.Extensions;
using StackExchange.Redis;

namespace Selector.Cache.Consumer.AppleMusic
{
    public class ApplePublisher(
        IAppleMusicPlayerWatcher watcher,
        ISubscriber subscriber,
        ILogger<ApplePublisher> logger = null,
        CancellationToken token = default)
        : BaseSequentialPlayerConsumer<IAppleMusicPlayerWatcher, AppleListeningChangeEventArgs>(watcher, logger),
            IApplePlayerConsumer
    {
        public CancellationToken CancelToken { get; set; } = token;

        protected override async Task ProcessEvent(AppleListeningChangeEventArgs e)
        {
            // using var scope = Logger.GetListeningEventArgsScope(e);

            var payload =
                ((AppleCurrentlyPlayingDTO)e).SerialiseBaggageWrapped(AppleJsonContext.Default
                    .AppleCurrentlyPlayingDTO);

            Logger.LogTrace("Publishing current");

            // TODO: currently using spotify username for cache key, use db username
            var receivers =
                await subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingAppleMusic(e.Id)), payload);

            Logger.LogDebug("Published current, {receivers} receivers", receivers);
        }
    }
}