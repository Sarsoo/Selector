using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BaseSequentialPlayerConsumer<TWatcher, TArgs>(
    TWatcher? watcher,
    ILogger<BaseSequentialPlayerConsumer<TWatcher, TArgs>> logger)
    : BasePlayerConsumer<TWatcher, TArgs>(watcher, logger), IProcessingConsumer<TArgs>
    where TWatcher : IWatcher<TArgs>
{
    protected new readonly ILogger<BaseSequentialPlayerConsumer<TWatcher, TArgs>> Logger = logger;

    private readonly Channel<TArgs> _events = Channel.CreateUnbounded<TArgs>();

    public override void Callback(object? sender, TArgs e)
    {
        if (!_events.Writer.TryWrite(e))
        {
            Logger.LogError("Failed to write event to channel");
        }
    }

    public Task ProcessQueue(CancellationToken token)
    {
        return Task.Run(async () =>
        {
            await foreach (var change in _events.Reader.ReadAllAsync(token))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await ProcessEvent(change);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to process event");
                }
            }
        }, cancellationToken: token);
    }
}