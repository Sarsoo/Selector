using Microsoft.Extensions.Logging;

namespace Selector;

public abstract class BasePlayerConsumer<TWatcher, TArgs>(
    TWatcher? watcher,
    ILogger<BasePlayerConsumer<TWatcher, TArgs>> logger) : IConsumer<TArgs>
    where TWatcher : IWatcher<TArgs>
{
    protected readonly ILogger<BasePlayerConsumer<TWatcher, TArgs>> Logger = logger;

    public abstract void Callback(object? sender, TArgs e);
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