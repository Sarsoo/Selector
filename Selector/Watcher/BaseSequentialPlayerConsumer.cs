using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BaseSequentialPlayerConsumer<TWatcher, TArgs>(
    TWatcher watcher,
    ILogger<BaseSequentialPlayerConsumer<TWatcher, TArgs>> logger) : IProcessingConsumer<TArgs>
    where TWatcher : IWatcher<TArgs>
{
    protected readonly ILogger<BaseSequentialPlayerConsumer<TWatcher, TArgs>> Logger = logger;

    private readonly Channel<TArgs> _events = Channel.CreateUnbounded<TArgs>();

    public void Callback(object sender, TArgs e)
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

    protected abstract Task ProcessEvent(TArgs e);

    public void Subscribe(IWatcher? watch = null)
    {
        var watcher1 = watch ?? watcher ?? throw new ArgumentNullException("No watcher provided");

        if (watcher1 is TWatcher watcherCast)
        {
            watcherCast.ItemChange += Callback;
        }
        else
        {
            throw new ArgumentException("Provided watcher is not a PlayerWatcher");
        }
    }

    public void Unsubscribe(IWatcher? watch = null)
    {
        var watcher1 = watch ?? watcher ?? throw new ArgumentNullException("No watcher provided");

        if (watcher1 is TWatcher watcherCast)
        {
            watcherCast.ItemChange -= Callback;
        }
        else
        {
            throw new ArgumentException("Provided watcher is not a PlayerWatcher");
        }
    }
}